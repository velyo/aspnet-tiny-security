using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Velyo.Web.Security.Tests
{
    [TestClass]
    public class ProfileProviderTests
    {
        [TestMethod]
        public void ProfileProvider_Initialize()
        {
            var provider = new ProfileProviderMock();
            var settings = new NameValueCollection();
            var name = "TestProfileProvider";
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
        public void ProfileProvider_Initialize_Name_Missing()
        {
            var provider = new ProfileProviderMock();

            provider.Initialize(null, null);
        }

        [TestMethod]
        public void ProfileProvider_Initialize_Settings_Default()
        {
            var provider = new ProfileProviderMock();
            var settings = new NameValueCollection();
            var name = "TestProfileProvider";

            provider.Initialize(name, settings);

            Assert.IsNull(provider.ApplicationName);
            Assert.AreEqual(false, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);
        }

        [TestMethod]
        public void ProfileProvider_Initialize_Settings_Missing()
        {
            var provider = new ProfileProviderMock();
            var name = "TestProfileProvider";

            provider.Initialize(name, null);

            Assert.IsNull(provider.ApplicationName);
            Assert.AreEqual(false, provider.CaseSensitive);
            Assert.AreEqual(name, provider.Description);
            Assert.AreEqual(name, provider.Name);
        }

        [TestMethod]
        public void ProfileProvider_GetPropertyValues()
        {
            var provider = new ProfileProviderMock();

            //provider.GetPropertyValues();

            Assert.Inconclusive("TODO");
        }

        [TestMethod]
        public void ProfileProvider_PrepareDataForSaving()
        {
            Assert.Inconclusive("TODO");
        }
    }
}
