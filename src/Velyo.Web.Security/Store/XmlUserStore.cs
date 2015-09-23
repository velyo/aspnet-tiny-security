using System.Collections.Generic;

namespace Alienlab.Web.Security.Store {

    /// <summary>
    /// 
    /// </summary>
    public class XmlUserStore : Persistable<List<XmlUser>> {

        #region Properties  ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <value>The users.</value>
        public virtual List<XmlUser> Users
        {
            get
            {
                return this.Value;
            }
        }
        #endregion

        #region Construct  ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlUserStore(string fileName)
            : base(fileName) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlUserStore"/> class.
        /// </summary>
        protected XmlUserStore()
            : base(null) {
        }
        #endregion

    }
}
