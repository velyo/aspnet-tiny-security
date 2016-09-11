using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;
using System.Text;
using System.ComponentModel;
using Velyo.Web.Security.Resources;

namespace Velyo.Web.Security
{
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
    public class XmlStore<T> : IDisposable where T : class, new()
    {
        private static readonly object FileChangedEvent = new object();
        private static readonly object ValueChangedEvent = new object();

        private T _value;

        [NonSerialized]
        private EventHandlerList _events;

        [NonSerialized]
        private readonly string _file;

        [NonSerialized]
        private readonly XmlSerializer _serializer;

        [NonSerialized]
        private readonly object _syncRoot = new object();


        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStore&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="file">Name of the file.</param>
        public XmlStore(string file) : this()
        {
            _file = file;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlStore&lt;T&gt;"/> class.
        /// </summary>
        protected internal XmlStore()
        {
            AutoCreate = true;
            AutoLoad = true;
            _serializer = new XmlSerializer(typeof(T));
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="XmlStore&lt;T&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~XmlStore()
        {
            Dispose(false);
        }


        /// <summary>
        /// Occurs when [file changed].
        /// </summary>
        public event EventHandler FileChanged
        {
            add
            {
                Events.AddHandler(FileChangedEvent, value);
            }
            remove
            {
                Events.RemoveHandler(FileChangedEvent, value);
            }
        }

        /// <summary>
        /// Occurs when [value changed].
        /// </summary>
        public event EventHandler ValueChanged
        {
            add
            {
                Events.AddHandler(ValueChangedEvent, value);
            }
            remove
            {
                Events.RemoveHandler(ValueChangedEvent, value);
            }
        }


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
        /// Gets the events.
        /// </summary>
        /// <value>The events.</value>
        protected EventHandlerList Events
        {
            get
            {
                return _events ?? (_events = new EventHandlerList());
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get
            {
                lock (SyncRoot)
                {
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
        public virtual T Value
        {
            get
            {
                if (IsEmpty)
                {
                    lock (SyncRoot)
                    {
                        if (IsEmpty)
                        {
                            if (AutoLoad) Load();
                            if (_value == null && AutoCreate) _value = new T();
                        }

                    }
                }
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    lock (SyncRoot)
                    {
                        if (_value != value)
                        {
                            _value = value;
                            OnValueChanged(EventArgs.Empty);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_events != null)
                {
                    _events.Dispose();
                    _events = null;
                }
                _value = null;
            }
        }


        /// <summary>
        /// Deletes the data from the cache and file system
        /// </summary>
        /// <returns></returns>
        public virtual bool Delete()
        {
            if (File.Exists(_file))
            {
                using (var scope = new FileLockScope(_file))
                {
                    // loop just in case of any file locks while deleting
                    for (int i = 1; i < 4; i++)
                    {
                        try
                        {
                            File.Delete(_file);
                            return true;
                        }
                        catch
                        {
                            Thread.Sleep(50 * i);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Loads the data from the file system. For deserialization an XmlSeralizer is used.
        /// </summary>
        public virtual void Load()
        {
            try
            {
                using (var scope = new FileLockScope(_file))
                {
                    if (File.Exists(_file))
                    {
                        using (FileStream reader = File.Open(_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            _value = (T)_serializer.Deserialize(reader);
                        }
                    }
                    AddFileNotifier();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Messages.LoadFailed, _file), ex);
            }
        }

        /// <summary>
        /// Persists the data back to the file system
        /// </summary>      
        public virtual void Save()
        {
            try
            {
                if (!IsEmpty)
                {
                    lock (SyncRoot)
                    {
                        if (!IsEmpty)
                        {
                            string file = DirectWrite ? _file : Path.GetTempFileName();

                            using (new FileLockScope(file))
                            {
                                FileMode mode = File.Exists(file) ? FileMode.Truncate : FileMode.OpenOrCreate;
                                using (FileStream stream = File.Open(file, mode, FileAccess.Write, FileShare.ReadWrite))
                                {
                                    _serializer.Serialize(stream, _value);
                                }

                                if (!DirectWrite)
                                {
                                    File.Copy(file, _file, true);
                                    File.Delete(file);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Messages.SaveFailed, _file), ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:FileChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnFileChanged(EventArgs e)
        {
            ((_events != null) ? _events[FileChangedEvent] as EventHandler : null)?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ValueChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnValueChanged(EventArgs e)
        {
            ((_events != null) ? _events[ValueChangedEvent] as EventHandler : null)?.Invoke(this, e);
        }

        #region File change notifier

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void AddFileNotifier()
        {
            HttpRuntime.Cache.Insert(
                _file,
                new object(),
                new CacheDependency(_file),
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
        void NotifyFileChanged(string key, object value, CacheItemRemovedReason reason)
        {
            Value = default(T);
            OnFileChanged(EventArgs.Empty);
        }
        #endregion


        class FileLockScope : IDisposable
        {
            private Mutex _lock;


            /// <summary>
            /// Initializes a new instance of the <see cref="XmlStore&lt;T&gt;.FileLockScope"/> class.
            /// </summary>
            /// <param name="fileName">Name of the file.</param>
            public FileLockScope(string fileName)
            {

                string fileNameHash = CalculateMD5Hash(fileName);
                _lock = new Mutex(false, fileNameHash);
                _lock.WaitOne();
            }

            /// <summary>
            /// Releases unmanaged resources and performs other cleanup operations before the
            /// <see cref="XmlStore&lt;T&gt;.FileLockScope"/> is reclaimed by garbage collection.
            /// </summary>
            ~FileLockScope()
            {
                Dispose(false);
            }


            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_lock != null)
                    {
                        _lock.ReleaseMutex();
                        _lock = null;
                    }
                }
            }

            /// <summary>
            /// Creates a MD5 hash from a string. Used for creating a unique name for the mutex's for each file that is accessed by the store objects. (putting the
            /// actual file name as a mutex name was resulting in a DirectoryNotFound error, which I believe was because of some special rules the
            /// Mutex name must follow, but aren't well documented. 
            /// </summary>
            /// <param name="strInput">Unhashed string</param>
            /// <returns>Hashed string</returns>
            private static string CalculateMD5Hash(string strInput)
            {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = Encoding.ASCII.GetBytes(strInput);
                byte[] hash = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
