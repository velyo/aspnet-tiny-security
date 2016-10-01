using System;
using System.Collections.Specialized;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            settings.Add("maxInvalidPasswordAttempts", "5");
            settings.Add("minRequiredNonAlphanumericCharacters", "0");
            settings.Add("minRequiredPasswordLength", "4");
            settings.Add("passwordAttemptWindow", "10");
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
            Assert.AreEqual(5, provider.MaxInvalidPasswordAttempts);
            Assert.AreEqual(0, provider.MinRequiredNonAlphanumericCharacters);
            Assert.AreEqual(4, provider.MinRequiredPasswordLength);
            Assert.AreEqual(10, provider.PasswordAttemptWindow);
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
        public void MembershipProvider_ChangePassword()
        {
        }

        [TestMethod]
        public void MembershipProvider_ChangePasswordQuestionAndAnswer()
        {
        }

        [TestMethod]
        public void MembershipProvider_DecodePassword()
        {
        }

        [TestMethod]
        public void MembershipProvider_EncodePassword()
        {
        }

        [TestMethod]
        public void MembershipProvider_GetPassword()
        {
        }

        [TestMethod]
        public void MembershipProvider_ResetPassword()
        {
        }

        [TestMethod]
        public void MembershipProvider_ValidateUser()
        {
        }

        [TestMethod]
        public void MembershipProvider_VerifyEmailIsUnique()
        {
        }

        [TestMethod]
        public void MembershipProvider_VerifyPasswordIsValid()
        {
        }

        [TestMethod]
        public void MembershipProvider_VerifyUserNameIsUnique()
        {
        }

        [TestMethod]
        public void MembershipProvider_VerifyUserIsValid()
        {
        }
    }
}
