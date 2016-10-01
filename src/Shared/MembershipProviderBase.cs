using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using Velyo.Web.Security.Resources;

namespace Velyo.Web.Security
{
    public abstract class MembershipProviderBase : MembershipProvider
    {
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        private string _passwordStrengthRegularExpression;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;


        /// <summary>
        /// Gets the sync root.
        /// </summary>
        /// <value>The sync root.</value>
        public static object SyncRoot { get; } = new object();

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [case sensitive].
        /// </summary>
        /// <value><c>true</c> if [case sensitive]; otherwise, <c>false</c>.</value>
        public virtual bool CaseSensitive { get; protected set; }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <value>The comparer.</value>
        protected virtual StringComparer Comparer { get; set; }

        /// <summary>
        /// Gets the comparison.
        /// </summary>
        /// <value>The comparison.</value>
        protected virtual StringComparison Comparison { get; set; }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.
        /// </returns>
        public override bool EnablePasswordReset { get { return _enablePasswordReset; } }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.
        /// </returns>
        public override bool EnablePasswordRetrieval { get { return _enablePasswordRetrieval; } }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </returns>
        public override int MaxInvalidPasswordAttempts { get { return _maxInvalidPasswordAttempts; } }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The minimum number of special characters that must be present in a valid password.
        /// </returns>
        public override int MinRequiredNonAlphanumericCharacters { get { return _minRequiredNonAlphanumericCharacters; } }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The minimum length required for a password.
        /// </returns>
        public override int MinRequiredPasswordLength { get { return _minRequiredPasswordLength; } }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow { get { return _passwordAttemptWindow; } }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.
        /// </returns>
        public override MembershipPasswordFormat PasswordFormat { get { return _passwordFormat; } }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A regular expression used to evaluate a password.
        /// </returns>
        public override string PasswordStrengthRegularExpression { get { return _passwordStrengthRegularExpression; } }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <value></value>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresQuestionAndAnswer { get { return _requiresQuestionAndAnswer; } }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresUniqueEmail { get { return _requiresUniqueEmail; } }

        /// <summary>
        /// Gets a value indicating whether [use universal time].
        /// </summary>
        /// <value><c>true</c> if [use universal time]; otherwise, <c>false</c>.</value>
        protected virtual bool UseUniversalTime { get; set; }


        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (oldPassword == null) throw new ArgumentNullException(nameof(oldPassword));
            if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));

            string password;
            string salt;
            string question;
            string answer;

            if (TryGetPassword(username, out password, out salt, out question, out answer))
            {
                // verify old password match
                oldPassword = EncodePassword(oldPassword, ref salt);
                if (string.Compare(oldPassword, password, Comparison) == 0)
                {
                    if (VerifyPasswordIsValid(newPassword))
                    {
                        newPassword = EncodePassword(newPassword, ref salt);
                        return TrySetPassword(username, newPassword, salt, question, answer);
                    }
                    else
                    {
                        throw new ProviderException(Messages.InvalidPassword);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (newPasswordQuestion == null) throw new ArgumentNullException(nameof(newPasswordQuestion));
            if (newPasswordAnswer == null) throw new ArgumentNullException(nameof(newPasswordAnswer));

            string pass;
            string salt;
            string question;
            string answer;

            if (TryGetPassword(username, out pass, out salt, out question, out answer))
            {
                // verify password match
                password = EncodePassword(password, ref salt);
                if (string.Compare(password, pass, Comparison) == 0)
                {
                    newPasswordAnswer = EncodePassword(newPasswordAnswer, ref salt);
                    return TrySetPassword(username, pass, salt, newPasswordQuestion, newPasswordAnswer);
                }
            }

            return false;
        }

        /// <summary>
        /// Decodes the password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns></returns>
        protected virtual string DecodePassword(string encodedPassword)
        {
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    return encodedPassword;
                case MembershipPasswordFormat.Encrypted:
                    byte[] encodedBytes = Convert.FromBase64String(encodedPassword);
                    byte[] bytes = DecryptPassword(encodedBytes);
                    return (bytes != null) ? Encoding.Unicode.GetString(bytes, 0x10, bytes.Length - 0x10) : null;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException(Messages.CannotDecodeHashedPassword);
                default:
                    throw new ProviderException(Messages.UnknownPasswordFormat);
            }
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        protected virtual string EncodePassword(string password, ref string salt)
        {
            if (PasswordFormat == MembershipPasswordFormat.Clear)
            {
                return password;
            }
            else if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
#pragma warning disable CS0618 // 'FormsAuthentication.HashPasswordForStoringInConfigFile(string, string)' is obsolete: 'The recommended alternative is to use the Membership APIs, such as Membership.CreateUser. For more information, see http://go.microsoft.com/fwlink/?LinkId=252463.'
                return FormsAuthentication.HashPasswordForStoringInConfigFile((salt + password), "SHA1");
#pragma warning restore CS0618 // 'FormsAuthentication.HashPasswordForStoringInConfigFile(string, string)' is obsolete: 'The recommended alternative is to use the Membership APIs, such as Membership.CreateUser. For more information, see http://go.microsoft.com/fwlink/?LinkId=252463.'
            }
            else if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                // Generate the salt if not passed in
                byte[] saltBytes;

                if (string.IsNullOrEmpty(salt))
                {
                    saltBytes = new byte[16];
                    RandomNumberGenerator rng = RandomNumberGenerator.Create();
                    rng.GetBytes(saltBytes);
                    salt = Convert.ToBase64String(saltBytes);
                }
                else
                {
                    saltBytes = Convert.FromBase64String(salt);
                }

                byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
                byte[] sourceBytes = new byte[passwordBytes.Length + saltBytes.Length];
                byte[] encodedBytes = null;

                Buffer.BlockCopy(saltBytes, 0, sourceBytes, 0, saltBytes.Length);
                Buffer.BlockCopy(passwordBytes, 0, sourceBytes, saltBytes.Length, passwordBytes.Length);

                encodedBytes = EncryptPassword(sourceBytes);
                return Convert.ToBase64String(encodedBytes);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        public override string GetPassword(string username, string passwordAnswer)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (!EnablePasswordRetrieval) throw new NotSupportedException(Messages.DisabledPasswordRetrieval);

            string password = null;
            string salt;
            string question;
            string answer;

            if (TryGetPassword(username, out password, out salt, out question, out answer))
            {
                if (RequiresQuestionAndAnswer)
                {
                    passwordAnswer = EncodePassword(passwordAnswer, ref salt);
                    if (string.Compare(passwordAnswer, answer, Comparison) != 0)
                    {
                        password = null;
                    }
                }
            }

            return (password != null) ? DecodePassword(password) : null;
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            string defaultAppName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            ApplicationName = config.GetString("applicationName", defaultAppName);

            // fetch provider settings
            _enablePasswordReset = config.GetBool("enablePasswordReset", true);
            _enablePasswordRetrieval = config.GetBool("enablePasswordRetrieval", false);
            _maxInvalidPasswordAttempts = config.GetInt("maxInvalidPasswordAttempts", 5);
            _minRequiredNonAlphanumericCharacters = config.GetInt("minRequiredNonAlphanumericCharacters", 0);
            _minRequiredPasswordLength = config.GetInt("minRequiredPasswordLength", 4);
            _passwordAttemptWindow = config.GetInt("passwordAttemptWindow", 10);
            _passwordFormat = config.GetEnum<MembershipPasswordFormat>("passwordFormat");
            _passwordStrengthRegularExpression = config.GetString("passwordStrengthRegularExpression", @"[\w| !§$%&/()=\-?\*]*");
            _requiresQuestionAndAnswer = config.GetBool("requiresQuestionAndAnswer", false);
            _requiresUniqueEmail = config.GetBool("requiresUniqueEmail", true);

            CaseSensitive = config.GetBool("caseSensitive", false);
            Comparer = CaseSensitive
                ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
            Comparison = CaseSensitive
                    ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            UseUniversalTime = config.GetBool("useUniversalTime", false);
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="passwordAnswer">The password answer for the specified user.</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string passwordAnswer)
        {

            if (username == null) throw new ArgumentNullException(nameof(username));
            if (!EnablePasswordReset) throw new NotSupportedException(Messages.DisabledPasswordReset);
            if (RequiresQuestionAndAnswer && (passwordAnswer == null)) throw new ArgumentException(Messages.RequiredPasswordQuestionAndAnswer);

            bool proceed = true;
            string password = null;
            string salt = null;
            string question = null;
            string answer;

            if (RequiresQuestionAndAnswer)
            {
                if (TryGetPassword(username, out password, out salt, out question, out answer))
                {
                    passwordAnswer = EncodePassword(passwordAnswer, ref salt);
                    if (string.Compare(passwordAnswer, answer, Comparison) != 0)
                    {
                        proceed = false;
                    }
                    else
                    {
                        password = null;
                        salt = null;
                    }
                }
            }

            if (proceed)
            {
                byte[] passwordBytes = new byte[16];

                RandomNumberGenerator rng = RandomNumberGenerator.Create();
                rng.GetBytes(passwordBytes);
                password = Convert.ToBase64String(passwordBytes);

                // TODO should we verify password is valid according to provider settings

                var encodedPassword = EncodePassword(password, ref salt);
                if (!TrySetPassword(username, encodedPassword, salt, question, passwordAnswer)) password = null;
            }

            return password;
        }

        /// <summary>
        /// Tries the get password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        protected abstract bool TryGetPassword(string username, out string password, out string salt, out string question, out string answer);

        /// <summary>
        /// Tries the set password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        protected abstract bool TrySetPassword(string username, string password, string salt, string question, string answer);

        /// <summary>
        /// Updates the user info.
        /// </summary>
        /// <param name="valid">if set to <c>true</c> [valid].</param>
        protected abstract void UpdateUserInfo(string username, bool valid);

        /// <summary>
        /// Validates the user internal.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override bool ValidateUser(string username, string password)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password));

            bool valid = false;

            if (VerifyPasswordIsValid(password))
            {
                var user = GetUser(username, false);

                if ((user != null) && user.IsApproved && !user.IsLockedOut)
                {
                    string pass;
                    string salt;
                    string question;
                    string answer;

                    if (TryGetPassword(username, out pass, out salt, out question, out answer))
                    {
                        password = EncodePassword(password, ref salt);
                        valid = (string.Compare(password, pass, Comparison) == 0);
                    }
                }
            }
            UpdateUserInfo(username, valid);

            return valid;
        }

        /// <summary>
        /// Verifies the email is unique.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="excludeKey">The exclude key.</param>
        /// <returns></returns>
        protected virtual bool VerifyEmailIsUnique(string email, Guid excludeKey)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            bool unique = true;
            int pageIndex = 0;
            int pageSize = int.MaxValue - 1;
            int totalRecords;
            Guid userKey;

            var matchedUsers = FindUsersByEmail(email, pageIndex, pageSize, out totalRecords);
            foreach (MembershipUser user in matchedUsers)
            {
                userKey = (Guid)user.ProviderUserKey;
                if (userKey.CompareTo(excludeKey) != 0)
                {
                    if (string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                    {
                        unique = false;
                        break;
                    }
                }
            }

            return unique;
        }

        /// <summary>
        /// Verifies the password is valid.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        protected virtual bool VerifyPasswordIsValid(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            bool valid = (password.Length >= MinRequiredPasswordLength);
            if (valid)
            {
                // Validate non-alphanumeric characters
                Regex regex = new Regex(@"\W", RegexOptions.Compiled);
                valid = (regex.Matches(password).Count >= MinRequiredNonAlphanumericCharacters);
                if (valid)
                {
                    // Validate strength regular expression
                    regex = new Regex(PasswordStrengthRegularExpression, RegexOptions.Compiled);
                    valid = valid && (regex.Matches(password).Count > 0);
                }
            }
            return valid;
        }

        /// <summary>
        /// Verifies the user name is unique.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="excludeKey">The exclude key.</param>
        /// <returns></returns>
        protected virtual bool VerifyUserNameIsUnique(string username, Guid excludeKey)
        {
            bool unique = true;
            int pageIndex = 0;
            int pageSize = int.MaxValue - 1;
            int totalRecords;
            Guid userKey;

            var matchedUsers = FindUsersByName(username, pageIndex, pageSize, out totalRecords);
            foreach (MembershipUser user in matchedUsers)
            {
                userKey = (Guid)user.ProviderUserKey;
                if (userKey.CompareTo(excludeKey) != 0)
                {
                    if (string.Equals(user.UserName, username, StringComparison.OrdinalIgnoreCase))
                    {
                        unique = false;
                        break;
                    }
                }
            }
            return unique;
        }

        /// <summary>
        /// Verifies the user is valid.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="email">The email.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        protected virtual bool VerifyUserIsValid(
            object userKey,
            string username,
            string password,
            string email,
            string question,
            string answer,
            out MembershipCreateStatus status)
        {
            // verify user key
            if ((userKey != null) && !(userKey is Guid))
            {
                status = MembershipCreateStatus.InvalidProviderUserKey;
                return false;
            }

            // verify username
            if (!VerifyUserNameIsUnique(username, Guid.Empty))
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return false;
            }

            // verify email
            if (RequiresUniqueEmail && !VerifyEmailIsUnique(email, Guid.Empty))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return false;
            }

            // verify password
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
            // Raise the event before validating the password
            base.OnValidatingPassword(e);
            if (e.Cancel || !VerifyPasswordIsValid(password))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return false;
            }

            // verify question and answer, if required
            if (RequiresQuestionAndAnswer)
            {
                if (string.IsNullOrEmpty(question) || question.Length > 0x100)
                {
                    status = MembershipCreateStatus.InvalidQuestion;
                    return false;
                }
                if (string.IsNullOrEmpty(answer) || (answer.Length > 0x80))
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return false;
                }
            }

            status = MembershipCreateStatus.Success;
            return true;
        }
    }
}
