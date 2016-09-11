using System.Collections.Generic;

namespace Velyo.Web.Security.Models
{
    public class Role
    {
        public string Name = string.Empty;
        public List<string> Users = new List<string>();
    }
}
