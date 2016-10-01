using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Velyo.Web.Security.Tests
{
    [TestClass]
    public class RoleProviderTests
    {
        [TestMethod]
        public void RoleProvider_Initialize()
        {
            var provider = new RoleProviderMock();
            var settings = new NameValueCollection();
            var name = "TestRoleProvider";
            var applicationName = "TestApplication";
            var caseSensitive = true;

            settings.Add(nameof(applicationName), applicationName);
            settings.Add(nameof(caseSensitive), caseSensitive.ToString().ToLower());

            provider.Initialize(name, settings);

            Assert.AreEqual(applicationName, provider.ApplicationName);
            Assert.AreEqual(caseSensitive, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RoleProvider_Initialize_Name_Missing()
        {
            var provider = new RoleProviderMock();

            provider.Initialize(null, null);
        }

        [TestMethod]
        public void RoleProvider_Initialize_Settings_Default()
        {
            var provider = new RoleProviderMock();
            var settings = new NameValueCollection();
            var name = "TestRoleProvider";

            provider.Initialize(name, settings);

            Assert.IsNull(provider.ApplicationName);
            Assert.AreEqual(false, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);
        }

        [TestMethod]
        public void RoleProvider_Initialize_Settings_Missing()
        {
            var provider = new RoleProviderMock();
            var name = "TestRoleProvider";

            provider.Initialize(name, null);

            Assert.IsNull(provider.ApplicationName);
            Assert.AreEqual(false, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);
        }
    }
}
