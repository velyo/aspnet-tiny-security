using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Velyo.Web.Security.Tests.Mocks;

namespace Velyo.Web.Security.Tests
{
    [TestClass]
    public class MembershipProviderTests
    {
        [TestMethod]
        public void MembershipProvider_Initialize()
        {
            var provider = new MembershipProviderMock();
            var settings = new NameValueCollection();
            var name = "TestMembershipProvider";
            var applicationName = "TestApplication";

            settings.Add(nameof(applicationName), applicationName);
            settings.Add("caseSensitive", "true");
            settings.Add("enablePasswordReset", "true");
            settings.Add("enablePasswordRetrieval", "true");
            settings.Add("maxInvalidPasswordAttempts", "10");
            settings.Add("minRequiredNonAlphanumericCharacters", "2");
            settings.Add("minRequiredPasswordLength", "5");
            settings.Add("passwordAttemptWindow", "8");
            settings.Add("passwordFormat", "Clear");
            settings.Add("passwordStrengthRegularExpression", "Test");
            settings.Add("requiresQuestionAndAnswer", "true");
            settings.Add("requiresUniqueEmail", "true");

            provider.Initialize(name, settings);

            Assert.AreEqual(applicationName, provider.ApplicationName);
            Assert.AreEqual(true, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);
            Assert.AreEqual(true, provider.EnablePasswordReset);
            Assert.AreEqual(true, provider.EnablePasswordRetrieval);
            Assert.AreEqual(10, provider.MaxInvalidPasswordAttempts);
            Assert.AreEqual(2, provider.MinRequiredNonAlphanumericCharacters);
            Assert.AreEqual(5, provider.MinRequiredPasswordLength);
            Assert.AreEqual(8, provider.PasswordAttemptWindow);
            Assert.AreEqual(MembershipPasswordFormat.Clear, provider.PasswordFormat);
            Assert.AreEqual("Test", provider.PasswordStrengthRegularExpression);
            Assert.AreEqual(true, provider.RequiresQuestionAndAnswer);
            Assert.AreEqual(true, provider.RequiresUniqueEmail);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MembershipProvider_Initialize_Name_Missing()
        {
            var provider = new MembershipProviderMock();

            provider.Initialize(null, null);
        }

        [TestMethod]
        public void MembershipProvider_Initialize_Settings_Default()
        {
            var provider = new MembershipProviderMock();
            var settings = new NameValueCollection();
            var name = "TestMembershipProvider";

            provider.Initialize(name, settings);

            Assert.IsNull(provider.ApplicationName);
            Assert.AreEqual(false, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);

            Assert.AreEqual(true, provider.EnablePasswordReset);
            Assert.AreEqual(false, provider.EnablePasswordRetrieval);
            Assert.AreEqual(5, provider.MaxInvalidPasswordAttempts);
            Assert.AreEqual(0, provider.MinRequiredNonAlphanumericCharacters);
            Assert.AreEqual(4, provider.MinRequiredPasswordLength);
            Assert.AreEqual(10, provider.PasswordAttemptWindow);
            Assert.AreEqual(MembershipPasswordFormat.Clear, provider.PasswordFormat);
            Assert.AreEqual(@"[\w| !§$%&/()=\-?\*]*", provider.PasswordStrengthRegularExpression);
            Assert.AreEqual(false, provider.RequiresQuestionAndAnswer);
            Assert.AreEqual(true, provider.RequiresUniqueEmail);
        }

        [TestMethod]
        public void MembershipProvider_Initialize_Settings_Missing()
        {
            var provider = new MembershipProviderMock();
            var name = "TestMembershipProvider";

            provider.Initialize(name, null);

            Assert.IsNull(provider.ApplicationName);
            Assert.AreEqual(false, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);

            Assert.AreEqual(true, provider.EnablePasswordReset);
            Assert.AreEqual(false, provider.EnablePasswordRetrieval);
            Assert.AreEqual(5, provider.MaxInvalidPasswordAttempts);
            Assert.AreEqual(0, provider.MinRequiredNonAlphanumericCharacters);
            Assert.AreEqual(4, provider.MinRequiredPasswordLength);
            Assert.AreEqual(10, provider.PasswordAttemptWindow);
            Assert.AreEqual(MembershipPasswordFormat.Clear, provider.PasswordFormat);
            Assert.AreEqual(@"[\w| !§$%&/()=\-?\*]*", provider.PasswordStrengthRegularExpression);
            Assert.AreEqual(false, provider.RequiresQuestionAndAnswer);
            Assert.AreEqual(true, provider.RequiresUniqueEmail);
        }

        [TestMethod]
        public void MembershipProvider_EncodePassword_Clear()
        {
            var provider = new MembershipProviderMock();
            var settings = new NameValueCollection();
            var name = "TestMembershipProvider";

            settings.Add("applicationName", "TestApplication");
            settings.Add("passwordFormat", "Clear");

            provider.Initialize(name, settings);

            var password = "test";
            string salt = null;
            string encodedPassword = provider.EncodePassword(password, ref salt);
            string decodedPassword = provider.DecodePassword(encodedPassword);

            Assert.AreEqual(password, decodedPassword);
        }

        [TestMethod]
        public void MembershipProvider_EncodePassword_Encrypted()
        {
            var provider = new MembershipProviderMock();
            var settings = new NameValueCollection();
            var name = "TestMembershipProvider";

            settings.Add("applicationName", "TestApplication");
            settings.Add("passwordFormat", "Encrypted");

            provider.Initialize(name, settings);

            var password = "test";
            string salt = null;
            string encodedPassword = provider.EncodePassword(password, ref salt);
            string decodedPassword = provider.DecodePassword(encodedPassword);

            Assert.AreEqual(password, decodedPassword);
        }

        [TestMethod]
        public void MembershipProvider_EncodePassword_Hashed()
        {
            var provider = new MembershipProviderMock();
            var settings = new NameValueCollection();
            var name = "TestMembershipProvider";

            settings.Add("applicationName", "TestApplication");
            settings.Add("passwordFormat", "Hashed");

            provider.Initialize(name, settings);

            var password = "test";
            string salt = null;
            string hashedPassword1 = provider.EncodePassword(password, ref salt);
            string hashedPassword2 = provider.EncodePassword(password, ref salt);

            Assert.AreEqual(hashedPassword1, hashedPassword2);
        }

        [TestMethod]
        public void MembershipProvider_ValidateUser()
        {
            var provider = new MembershipProviderMock();
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void MembershipProvider_VerifyEmailIsUnique()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void MembershipProvider_VerifyPasswordIsValid()
        {
            var provider = new MembershipProviderMock();
            var settings = new NameValueCollection();
            var name = "TestMembershipProvider";

            settings.Add("minRequiredNonAlphanumericCharacters", "2");

            provider.Initialize(name, settings);

            Assert.IsFalse(provider.VerifyPasswordIsValid("ABC"));
            Assert.IsFalse(provider.VerifyPasswordIsValid("ABCDE"));
            Assert.IsFalse(provider.VerifyPasswordIsValid("ABC12"));
            Assert.IsTrue(provider.VerifyPasswordIsValid("ABC12!?"));
        }

        [TestMethod]
        public void MembershipProvider_VerifyUserNameIsUnique()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void MembershipProvider_VerifyUserIsValid()
        {
            var provider = new MembershipProviderMock();
            Assert.Inconclusive("Not implemented");
        }
    }
}
