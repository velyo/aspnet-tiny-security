using System.Collections.Generic;

namespace Alienlab.Web.Security.Store {

    /// <summary>
    /// 
    /// </summary>
    public partial class XmlProfileStore : Persistable<List<XmlProfile>> {

        #region Properties  /////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        public virtual List<XmlProfile> Profiles { get { return base.Value; } }

        #endregion

        #region Construct  //////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public XmlProfileStore(string fileName)
            : base(fileName) {
        }

        /// <summary>
        /// 
        /// </summary>
        protected XmlProfileStore()
            : base(null) {
        }
        #endregion
    }
}
