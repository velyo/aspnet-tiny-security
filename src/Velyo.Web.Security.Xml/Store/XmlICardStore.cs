using System;
using System.Collections.Generic;
using System.Text;
using Velyo.Web.Security.Models;

namespace Velyo.Web.Security.Store
{
    /// <summary>
    /// TODO change XmlUserStore Delete to remove the associated ICard, if any.
    /// </summary>
    public class XmlCardStore : XmlStore<List<Card>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlCardStore"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlCardStore(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlCardStore"/> class.
        /// </summary>
        public XmlCardStore()
            : base(null)
        {
        }

        /// <summary>
        /// Gets the entire collection of information cards.
        /// </summary>
        /// <value>The cards.</value>
        public virtual List<Card> Cards
        {
            get
            {
                return Value;
            }
        }


        /// <summary>
        /// Associates the specified user to specified information card.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="uniqueID">The unique ID.</param>
        /// <param name="ppID">The pp ID.</param>
        public void Associate(string userName, string uniqueID, string ppID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the information card PPID by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public string FindPPIDForUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the information card PPID and unique ID by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="uniqueID">The unique ID.</param>
        /// <returns></returns>
        public string FindPPIDForUser(string userName, out string uniqueID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the information card by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public XmlCardStore GetByUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lookups by specified unique card ID and returns the coresponding user name.
        /// </summary>
        /// <param name="uniqueID">The unique ID.</param>
        /// <returns></returns>
        public string Lookup(string uniqueID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the information card specified by its unique ID.
        /// </summary>
        /// <param name="uniqueID">The unique ID.</param>
        public void Remove(string uniqueID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the association of the specified user to specified information card.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="ppID">The pp ID.</param>
        public void UnAssociate(string userName, string ppID)
        {
            throw new NotImplementedException();
        }
    }
}
