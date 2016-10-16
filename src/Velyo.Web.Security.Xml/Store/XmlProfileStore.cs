using System.Collections.Generic;
using Velyo.Web.Security.Models;

namespace Velyo.Web.Security.Store
{
    /// <summary>
    /// 
    /// </summary>
    public partial class XmlProfileStore : XmlStore<List<Profile>>
    {
        /// <summary>
        /// 
        /// </summary>
        public XmlProfileStore(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected XmlProfileStore()
            : base(null)
        {
        }

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        public virtual List<Profile> Profiles
        {
            get
            {
                return Value;
            }
        }
    }
}
