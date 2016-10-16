using System.Collections.Generic;
using Velyo.Web.Security.Models;

namespace Velyo.Web.Security.Store
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlRoleStore : XmlStore<List<Role>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRoleStore"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlRoleStore(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRoleStore"/> class.
        /// </summary>
        protected XmlRoleStore()
            : base(null)
        {
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public virtual List<Role> Roles
        {
            get
            {
                return Value;
            }
        }
    }
}
