using System;

namespace Alienlab.Web.Security.Store {

    /// <summary>
    /// 
    /// </summary>
    public class XmlProfile {

        public string UserName;
        public string Names = string.Empty;
        public string ValuesString = string.Empty;
        public string ValuesBinary = null;
        public DateTime LastUpdated = DateTime.MinValue;
        public bool Authenticated = true;
    }
}