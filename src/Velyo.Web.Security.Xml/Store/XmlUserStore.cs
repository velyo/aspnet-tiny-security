using System.Collections.Generic;
using Velyo.Web.Security.Models;

namespace Velyo.Web.Security.Store
{

    /// <summary>
    /// 
    /// </summary>
    public class XmlUserStore : XmlStore<List<User>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlUserStore(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlUserStore"/> class.
        /// </summary>
        protected XmlUserStore()
            : base(null)
        {
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <value>The users.</value>
        public virtual List<User> Users
        {
            get
            {
                return Value;
            }
        }
    }
}
