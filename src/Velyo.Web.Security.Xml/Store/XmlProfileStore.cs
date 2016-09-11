using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

namespace Velyo.Web.Security.Store {

    /// <summary>
    /// 
    /// </summary>
    public partial class XmlProfileStore : XmlStore<List<XmlProfile>> {

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
