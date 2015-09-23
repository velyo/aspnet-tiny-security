using System;

namespace Alienlab.Web.Security.Store {

    /// <summary>
    /// The Information Card XML data.
    /// </summary>
    public class XmlICard {

        // key
        public Guid UserKey = Guid.Empty;
        // ids
        public string UniqueID;
        public string PPID;
        // claims
        public string FirstName;
        public string LastName;
        public string StreetAddress;
        public string Locality;
        public string StateOrProvince;
        public string PostalCode;
        public string Country;
        public string HomePhone;
        public string OtherPhone;
        public string MobilePhone;
        public DateTime DateOfBirth;
        public Gender Gender;
        public string WebPage;

    }
}
