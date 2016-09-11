using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.Profile;
using System.Web.Security;
using Velyo.Web.Security.Store;

namespace Velyo.Web.Security
{
    /// <summary>
    /// Specialized MembershipProvider that uses a file (Users.config) to store its data.
    /// Passwords for the users are always stored as a salted hash (see: http://msdn.microsoft.com/msdnmag/issues/03/08/SecurityBriefs/)
    /// </summary>
    public class XmlMembershipProvider : MembershipProviderBase, IDisposable
    {
        private string _file;
        private WeakReference _storeRef;


        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="XmlMembershipProvider"/> is reclaimed by garbage collection.
        /// </summary>
        ~XmlMembershipProvider()
        {
            Dispose(false);
        }


        /// <summary>
        /// Gets the store.
        /// </summary>
        /// <value>The store.</value>
        protected XmlUserStore Store
        {
            get
            {
                XmlUserStore store = StoreRef.Target as XmlUserStore;
                if (store == null)
                {
                    StoreRef.Target = store = new XmlUserStore(_file);
                }
                return store;
            }
        }

        /// <summary>
        /// Gets the store ref.
        /// </summary>
        /// <value>The store ref.</value>
        private WeakReference StoreRef
        {
            get
            {
                return _storeRef ?? (_storeRef = new WeakReference(new XmlUserStore(_file)));
            }
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <value>The users.</value>
        public List<XmlUser> Users { get { return Store.Users; } }


        /// <summary>
        /// Setups the specified saved state.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        private static void SetupInitialUser(string role, string user, string password)
        {
            if (Roles.Enabled)
            {
                if (!Roles.RoleExists(role)) Roles.CreateRole(role);
                Membership.CreateUser(user, password);
                Roles.AddUserToRole(user, role);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_storeRef != null)
                {
                    var store = _storeRef.Target as XmlUserStore;
                    if (store != null) store.Dispose();
                    _storeRef.Target = store = null;
                    _storeRef = null;
                }
                _file = null;
            }
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"></see> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the information for the newly created user.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when username is <c>null</c></exception>
        public override MembershipUser CreateUser(
            string username,
            string password,
            string email,
            string passwordQuestion,
            string passwordAnswer,
            bool isApproved,
            object providerUserKey,
            out MembershipCreateStatus status)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            try
            {
                bool valid = VerifyUserIsValid(
                    providerUserKey, username, password, email, passwordQuestion, passwordAnswer, out status);

                if (valid)
                {
                    // user date is valid then create
                    DateTime now = UseUniversalTime ? DateTime.UtcNow : DateTime.Now;
                    string salt = string.Empty;
                    string encodedPassword = EncodePassword(password, ref salt);

                    var userKey = (providerUserKey != null) ? (Guid)providerUserKey : Guid.NewGuid();
                    XmlUser user = new XmlUser
                    {
                        UserKey = userKey,
                        UserName = username,
                        PasswordSalt = salt,
                        Password = encodedPassword,
                        Email = email,
                        PasswordQuestion = passwordQuestion,
                        PasswordAnswer = passwordAnswer,
                        IsApproved = isApproved,
                        CreationDate = now,
                        LastActivityDate = now,
                        LastPasswordChangeDate = now
                    };

                    lock (SyncRoot)
                    {
                        // Add the user to the store
                        Store.Users.Add(user);
                        Store.Save();
                    }

                    return CreateMembershipFromInternalUser(user);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when username is <c>null</c></exception>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            try
            {
                if (deleteAllRelatedData)
                {
                    // remove user from roles
                    string[] roles = Roles.GetRolesForUser(username);
                    if ((roles != null) && (roles.Length > 0))
                    {
                        Roles.RemoveUsersFromRoles(new string[] { username }, roles);
                    }
                    // delete user profile
                    ProfileManager.DeleteProfile(username);
                }

                lock (SyncRoot)
                {
                    XmlUser user = GetInternalUser(username);
                    if (user != null)
                    {
                        Store.Users.Remove(user);
                        Store.Save();
                    }
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when emailToMatch is <c>null</c></exception>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (emailToMatch == null) throw new ArgumentNullException(nameof(emailToMatch));

            XmlUser[] users;
            int pageOffset = pageIndex * pageSize;

            try
            {
                var comparison = Comparison;
                var query = from u in Users
                            let email = u.Email
                            where ((email != null) && (email.IndexOf(emailToMatch, comparison) >= 0))
                            select u;

                lock (SyncRoot)
                {
                    totalRecords = query.Count();
                    users = query.Skip(pageOffset).Take(pageSize).ToArray();
                }

                return CreateMembershipCollectionFromInternalList(users);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when usernameToMatch is <c>null</c></exception>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (usernameToMatch == null) throw new ArgumentNullException(nameof(usernameToMatch));

            int pageOffset = pageIndex * pageSize;
            XmlUser[] users;

            try
            {
                var comparison = Comparison;
                var query = from u in Users
                            let name = u.UserName
                            where (name.IndexOf(usernameToMatch, comparison) >= 0)
                            select u;

                lock (SyncRoot)
                {
                    totalRecords = query.Count();
                    users = query.Skip(pageOffset).Take(pageSize).ToArray();
                }

                return CreateMembershipCollectionFromInternalList(users);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            int pageOffset = pageIndex * pageSize;
            XmlUser[] users;

            try
            {
                lock (SyncRoot)
                {
                    totalRecords = Users.Count;
                    users = Users.Skip(pageOffset).Take(pageSize).ToArray();
                }

                return CreateMembershipCollectionFromInternalList(users);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            int count = 0;
            int onlineTime = Membership.UserIsOnlineTimeWindow;
            DateTime now = UseUniversalTime ? DateTime.UtcNow : DateTime.Now;

            try
            {
                var query = from u in Users
                            where (u.LastActivityDate.AddMinutes(onlineTime) >= now)
                            select u;

                lock (SyncRoot)
                {
                    count = query.Count();
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when username is <c>null</c></exception>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            XmlUser user;

            try
            {
                lock (SyncRoot)
                {
                    user = GetInternalUser(username);
                    if ((user != null) && (userIsOnline))
                    {
                        user.LastActivityDate = UseUniversalTime ? DateTime.UtcNow : DateTime.Now;
                        Store.Save();
                    }
                }

                return CreateMembershipFromInternalUser(user);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets information from the data source for a user based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when providerUserKey is <c>null</c></exception>
        /// <exception cref="T:System.ArgumentException">Thrown when providerUserKey is not a <see cref="T:System.Guid"></see></exception>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null) throw new ArgumentNullException(nameof(providerUserKey));
            if (!(providerUserKey is Guid)) throw new ArgumentException("Invalid provider user key. Must be a Guid.");

            XmlUser user;
            Guid key = (Guid)providerUserKey;

            try
            {
                var query = from u in Users
                            where (u.UserKey == key)
                            select u;

                lock (SyncRoot)
                {
                    user = query.FirstOrDefault();
                    if ((user != null) && (userIsOnline))
                    {
                        user.LastActivityDate = DateTime.Now;
                        Store.Save();
                    }
                }

                return CreateMembershipFromInternalUser(user);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when email is <c>null</c></exception>
        public override string GetUserNameByEmail(string email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            XmlUser user;

            try
            {
                var comparison = Comparison;
                var query = from u in Users
                            where u.Email.Equals(email, comparison)
                            select u;

                lock (SyncRoot)
                {
                    if (RequiresUniqueEmail && (query.Count() > 1))
                        throw new ProviderException("More than one user with same email found.");
                    user = query.FirstOrDefault();
                }

                return (user != null) ? user.UserName : null;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when username is <c>null</c></exception>
        public override bool UnlockUser(string username)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            bool unlocked = false;

            try
            {
                lock (SyncRoot)
                {
                    var user = GetInternalUser(username);
                    if (user != null && user.IsLockedOut)
                    {
                        user.IsLockedOut = false;
                        user.FailedPasswordAttemptCount = 0;
                        Store.Save();
                    }
                }
            }
            catch
            {
                throw;
            }

            return unlocked;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when user is <c>null</c></exception>
        public override void UpdateUser(MembershipUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // TODO should we allow username to be changed as well
            //if (this.VerifyUserNameIsUnique(user.UserName, (Guid)user.ProviderUserKey))
            //    throw new ArgumentException("UserName is not unique!");
            if (RequiresUniqueEmail && !VerifyEmailIsUnique(user.Email, (Guid)user.ProviderUserKey))
            {
                throw new ArgumentException("Email is not unique!");
            }

            lock (SyncRoot)
            {
                var xuser = GetInternalUser(user.UserName);

                if (xuser != null)
                {
                    xuser.Email = user.Email;
                    xuser.LastActivityDate = user.LastActivityDate;
                    xuser.LastLoginDate = user.LastLoginDate;
                    xuser.Comment = user.Comment;
                    xuser.IsApproved = user.IsApproved;
                    Store.Save();
                }
                else
                {
                    throw new ProviderException("User does not exist!");
                }
            }
        }

        #region - Helpers -

        /// <summary>
        /// Creates the membership from internal user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private MembershipUser CreateMembershipFromInternalUser(XmlUser user)
        {
            return (user != null)
                ? new MembershipUser(Name,
                    user.UserName, user.UserKey, user.Email, user.PasswordQuestion,
                    user.Comment, user.IsApproved, user.IsLockedOut, user.CreationDate, user.LastLoginDate,
                    user.LastActivityDate, user.LastPasswordChangeDate, user.LastLockoutDate)
                : null;
        }

        /// <summary>
        /// Creates the membership collection from internal list.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        private MembershipUserCollection CreateMembershipCollectionFromInternalList(IEnumerable<XmlUser> users)
        {
            MembershipUserCollection returnCollection = new MembershipUserCollection();

            foreach (XmlUser user in users)
            {
                returnCollection.Add(CreateMembershipFromInternalUser(user));
            }

            return returnCollection;
        }

        /// <summary>
        /// Gets the internal user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        private XmlUser GetInternalUser(string username)
        {
            var comparison = Comparison;
            var query = from u in Users
                        where u.UserName.Equals(username, comparison)
                        select u;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Tries the get password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="question"></param>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        protected override bool TryGetPassword(string username, out string password, out string salt, out string question, out string answer)
        {
            lock (SyncRoot)
            {
                XmlUser user = GetInternalUser(username);
                if (user != null)
                {
                    password = user.Password;
                    salt = user.PasswordSalt;
                    question = user.PasswordQuestion;
                    answer = user.PasswordAnswer;
                    return true;
                }
                else
                {
                    password = salt = question = answer = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Tries the set password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="question"></param>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        protected override bool TrySetPassword(string username, string password, string salt, string question, string answer)
        {
            lock (SyncRoot)
            {
                XmlUser user = GetInternalUser(username);
                if (user != null)
                {
                    user.LastPasswordChangeDate = UseUniversalTime ? DateTime.UtcNow : DateTime.Now;
                    user.Password = password;
                    user.PasswordSalt = salt;
                    user.PasswordQuestion = question;
                    user.PasswordAnswer = answer;
                    Store.Save();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Updates the user info.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="valid">if set to <c>true</c> [valid].</param>
        protected override void UpdateUserInfo(string username, bool valid)
        {
            try
            {
                lock (SyncRoot)
                {
                    var user = GetInternalUser(username);
                    if (user != null)
                    {
                        DateTime now = UseUniversalTime ? DateTime.UtcNow : DateTime.Now;

                        user.LastActivityDate = now;
                        if (valid)
                        {
                            user.LastLoginDate = now;
                            user.FailedPasswordAttemptCount = 0;
                        }
                        else
                        {
                            user.FailedPasswordAttemptCount++;
                            if (!user.IsLockedOut)
                            {
                                user.IsLockedOut = (user.FailedPasswordAttemptCount >= MaxInvalidPasswordAttempts);
                                if (user.IsLockedOut)
                                    user.LastLockoutDate = now;
                            }
                        }
                        Store.Save();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // prerequisites
            if (string.IsNullOrEmpty(name))
            {
                name = "XmlMembershipProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML Membership Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            // initialize provider fields
            string fileName = config.GetString("fileName", "Users.xml");
            string folder = config.GetString("folder", "~/App_Data/");

            if (!folder.EndsWith("/")) folder += "/";
            _file = HostingEnvironment.MapPath(string.Format("{0}{1}", folder, fileName));
        }
    }

    
    public sealed class SaltedHash
    {
        private readonly string _salt;
        private readonly string _hash;
        private const int saltLength = 6;

        public string Salt { get { return _salt; } }
        public string Hash { get { return _hash; } }

        public static SaltedHash Create(string password)
        {
            string salt = _createSalt();
            string hash = _calculateHash(salt, password);
            return new SaltedHash(salt, hash);
        }

        public static SaltedHash Create(string salt, string hash)
        {
            return new SaltedHash(salt, hash);
        }

        public bool Verify(string password)
        {
            string h = _calculateHash(_salt, password);
            return _hash.Equals(h);
        }

        private SaltedHash(string s, string h)
        {
            _salt = s;
            _hash = h;
        }

        private static string _createSalt()
        {
            byte[] r = _createRandomBytes(saltLength);
            return Convert.ToBase64String(r);
        }

        private static byte[] _createRandomBytes(int len)
        {
            byte[] r = new byte[len];
            new RNGCryptoServiceProvider().GetBytes(r);
            return r;
        }

        private static string _calculateHash(string salt, string password)
        {
            byte[] data = _toByteArray(salt + password);
            byte[] hash = _calculateHash(data);
            return Convert.ToBase64String(hash);
        }

        private static byte[] _calculateHash(byte[] data)
        {
            return new SHA1CryptoServiceProvider().ComputeHash(data);
        }

        private static byte[] _toByteArray(string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
    }
}