using System;

namespace Velyo.Web.Security.Models
{
    [Serializable]
    public class Profile
    {
        public string UserName { get; set; }
        public string Names { get; set; } = string.Empty;
        public string ValuesString { get; set; } = string.Empty;
        public string ValuesBinary { get; set; } = null;
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public bool Authenticated { get; set; } = true;
    }
}