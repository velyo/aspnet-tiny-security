using System.Collections.Generic;

namespace Alienlab.Web.Security.Store {

    /// <summary>
    /// 
    /// </summary>
    public class XmlRoleStore : Persistable<List<XmlRole>> {

        #region Properties  ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public virtual List<XmlRole> Roles { get { return base.Value; } }

        #endregion

        #region Construct  ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRoleStore"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlRoleStore(string fileName)
            : base(fileName) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRoleStore"/> class.
        /// </summary>
        protected XmlRoleStore()
            : base(null) {
        }
        #endregion

    }
}
