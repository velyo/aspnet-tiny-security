using System;
using System.Collections;
using System.IO;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;
using System.Text;
using System.ComponentModel;
using Velyo.Web.Security.Resources;

namespace Velyo {

    /// <summary>
    /// Generic class for a perssitable object.
    /// Encapsulates the logic to load/save data from/to the filesystem.
    /// To speed up the acces, caching is used.
    /// </summary>
    /// <typeparam name="T">class or struct with all the data-fields that must be persisted</typeparam>
    /// <summary>
    /// Generic class for a perssitable object.
    /// Encapsulates the logic to load/save data from/to the filesystem.
    /// To speed up the acces, caching is used.
    /// </summary>
    /// <typeparam name="T">class or struct with all the data-fields that must be persisted</typeparam>
    public class Persistable<T> : IDisposable where T : class, new() {

        #region Static Fields /////////////////////////////////////////////////////////////////////

        static readonly object FileChangedEvent = new object();
        static readonly object ValueChangedEvent = new object();

        #endregion

        #region Fields  ///////////////////////////////////////////////////////////////////////////

        [NonSerialized]
        bool _disposed;
        [NonSerialized]
        EventHandlerList _events;
        [NonSerialized]
        readonly string _file;

        [NonSerialized]
        readonly XmlSerializer _serializer;
        [NonSerialized]
        readonly object _syncRoot = new object();

        #endregion

        #region Properties  ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a value indicating whether [auto create].
        /// </summary>
        /// <value><c>true</c> if [auto create]; otherwise, <c>false</c>.</value>
        public bool AutoCreate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto load].
        /// </summary>
        /// <value><c>true</c> if [auto load]; otherwise, <c>false</c>.</value>
        public bool AutoLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [direct write].
        /// </summary>
        /// <value><c>true</c> if [direct write]; otherwise, <c>false</c>.</value>
        public bool DirectWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Persistable&lt;T&gt;"/> is disposed.
        /// </summary>
        /// <value><c>true</c> if disposed; otherwise, <c>false</c>.</value>
        protected bool Disposed {
            get {
                lock (this) {
                    return _disposed;
                }
            }
        }

        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <value>The events.</value>
        protected EventHandlerList Events {
            get {
                if (_events == null)
                    _events = new EventHandlerList();
                return _events;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty {
            get {
                lock (SyncRoot) {
                    return (_value == null || (_value is ICollection && ((ICollection)_value).Count == 0));
                }

            }
        }

        /// <summary>
        /// Gets or sets the sync root.
        /// </summary>
        /// <value>The sync root.</value>
        public virtual object SyncRoot { get { return _syncRoot; } }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public virtual T Value {
            get {
                lock (SyncRoot) {
                    if (this.IsEmpty) {
                        if (this.AutoLoad) Load();
                        if (_value == null && this.AutoCreate) _value = new T();
                    }
                    return _value;
                }
            }
            set {
                lock (SyncRoot) {
                    _value = value;
                    this.OnValueChanged(EventArgs.Empty);
                }
            }
        }
        T _value;

        #endregion

        #region Events ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when [file changed].
        /// </summary>
        public event EventHandler FileChanged {
            add {
                this.Events.AddHandler(FileChangedEvent, value);
            }
            remove {
                this.Events.RemoveHandler(FileChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs when [value changed].
        /// </summary>
        public event EventHandler ValueChanged {
            add {
                this.Events.AddHandler(ValueChangedEvent, value);
            }
            remove {
                this.Events.RemoveHandler(ValueChangedEvent, value);
            }
        }

        #endregion

        #region Construct / Destruct //////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="Persistable&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="file">Name of the file.</param>
        public Persistable(string file)
            : this() {
            this._file = file;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Persistable&lt;T&gt;"/> class.
        /// </summary>
        protected internal Persistable() {

            this.AutoCreate = true;
            this.AutoLoad = true;
            _serializer = new XmlSerializer(typeof(T));
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Persistable&lt;T&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~Persistable() {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) {

            lock (this) {
                if (!_disposed) {
                    _disposed = true;
                    if (disposing) {
                        if (_events != null) _events.Dispose();
                        _value = default(T);
                    }
                    // release unmanaged resources
                }
            }
        }
        #endregion

        #region Methods /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Deletes the data from the cache and filesystem
        /// </summary>
        /// <returns></returns>
        public virtual bool Delete() {

            bool success = false;

            using (var scope = new FileLockScope(this._file)) {
                if (File.Exists(this._file)) {
                    // loop just in case of any file locks while deleting
                    for (int i = 1; i < 4; i++) {
                        try {
                            File.Delete(this._file);
                            success = true;
                        }
                        catch {
                            Thread.Sleep(50 * i);
                        }
                    }
                }
            }

            return success;
        }


        /// <summary>
        /// Loads the data from the filesystem. For deserialization an XmlSeralizer is used.
        /// </summary>
        public virtual void Load() {

            lock (SyncRoot) {
                string file = this._file;

                using (var scope = new FileLockScope(file)) {
                    try {
                        if (System.IO.File.Exists(file)) {
                            using (FileStream reader = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                                _value = (T)_serializer.Deserialize(reader);
                            }
                        }
                        AddFileNotifier();
                    }
                    catch (Exception ex) {
                        throw new Exception(string.Format(Messages.LoadFailed, file), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Persists the data back to the filesystem
        /// </summary>      
        public virtual void Save() {

            lock (SyncRoot) {
                if (!this.IsEmpty) {
                    string file = this.DirectWrite ? _file : Path.GetTempFileName();
                    var value = _value;

                    using (var scope = new FileLockScope(file)) {
                        try {

                            FileMode mode = File.Exists(file) ? FileMode.Truncate : FileMode.OpenOrCreate;
                            using (FileStream stream = File.Open(file, mode, FileAccess.Write, FileShare.ReadWrite)) {
                                _serializer.Serialize((Stream)stream, value);
                            }

                            if (!this.DirectWrite) {
                                File.Copy(file, _file, true);
                                File.Delete(file);
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception(string.Format(Messages.SaveFailed, file), ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:FileChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnFileChanged(EventArgs e) {
            EventHandler handler = (_events != null) ? _events[FileChangedEvent] as EventHandler : null;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ValueChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnValueChanged(EventArgs e) {
            EventHandler handler = (_events != null) ? _events[ValueChangedEvent] as EventHandler : null;
            if (handler != null) handler(this, e);
        }

        #region File change notifier

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void AddFileNotifier() {

            string file = this._file;
            HttpRuntime.Cache.Insert(
                file,
                new object(),
                new CacheDependency(file),
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                new CacheItemRemovedCallback(NotifyFileChanged));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="reason"></param>
        void NotifyFileChanged(string key, object value, CacheItemRemovedReason reason) {
            Value = default(T);
            this.OnFileChanged(EventArgs.Empty);
        }
        #endregion
        #endregion

        #region Nested Types //////////////////////////////////////////////////////////////////////

        class FileLockScope : IDisposable {

            #region Properties

            protected Mutex Lock { get; set; }

            #endregion

            #region Ctor/Dtor

            /// <summary>
            /// Initializes a new instance of the <see cref="Persistable&lt;T&gt;.FileLockScope"/> class.
            /// </summary>
            /// <param name="fileName">Name of the file.</param>
            public FileLockScope(string fileName) {

                string fileNameHash = CalculateMD5Hash(fileName);
                this.Lock = new Mutex(false, fileNameHash);
                this.Lock.WaitOne();
            }

            /// <summary>
            /// Releases unmanaged resources and performs other cleanup operations before the
            /// <see cref="Persistable&lt;T&gt;.FileLockScope"/> is reclaimed by garbage collection.
            /// </summary>
            ~FileLockScope() {
                Dispose(false);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing) {
                if (this.Lock != null) {
                    this.Lock.ReleaseMutex();
                    this.Lock = null;
                }
            }
            #endregion

            #region Methods

            /// <summary>
            /// Creates a MD5 hash from a string. Used for creating a unique name for the mutex's for each file that is accessed by the store objects. (putting the
            /// actual file name as a mutex name was resulting in a DirectoryNotFound error, which I believe was because of some special rules the
            /// Mutex name must follow, but aren't well documented. 
            /// </summary>
            /// <param name="strInput">Unhashed string</param>
            /// <returns>Hashed string</returns>
            private static string CalculateMD5Hash(string strInput) {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = Encoding.ASCII.GetBytes(strInput);
                byte[] hash = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++) {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
            #endregion
        }
        #endregion
    }
}
