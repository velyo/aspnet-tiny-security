using System;

namespace Velyo.Web.Security.Models
{
    [Serializable]
    public class Card
    {
        // key
        public Guid UserKey { get; set; } = Guid.Empty;
        // ids
        public string UniqueID { get; set; }
        public string PPID { get; set; }
        // claims
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public string Locality { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string OtherPhone { get; set; }
        public string MobilePhone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string WebPage { get; set; }
    }
}
