using System;

namespace Velyo.Web.Security.Models
{
    [Serializable]
    public class User
    {
        public Guid UserKey { get; set; } = Guid.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordQuestion { get; set; } = string.Empty;
        public string PasswordAnswer { get; set; } = string.Empty;
        public string Comment { get; set; }
        // track data
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastActivityDate { get; set; } = DateTime.MinValue;
        public DateTime LastLoginDate { get; set; } = DateTime.MinValue;
        public DateTime LastPasswordChangeDate { get; set; } = DateTime.MinValue;
        public DateTime LastLockoutDate { get; set; } = DateTime.MaxValue;
        public bool IsApproved { get; set; } = true;
        public bool IsLockedOut { get; set; } = false;
        public int FailedPasswordAttemptCount { get; set; }
    }
}
