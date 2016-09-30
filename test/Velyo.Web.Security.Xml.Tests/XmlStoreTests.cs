using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Velyo.Web.Security;

namespace Artem.Web.Security.Xml.Tests
{
    /// <summary>
    ///This is a test class for PersistableTest and is intended
    ///to contain all PersistableTest Unit Tests
    ///</summary>
    [TestClass]
    public class XmlStoreTests
    {
        private static string Path { get; set; }


        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Path = string.Format(@"{0}\Test.xml", testContext.TestDir);
        }


        [TestMethod]
        public void XmlStore_Delete()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void XmlStore_Load()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void XmlStore_Save_Batch()
        {
            var store = new XmlStore<People>(Path);

            for (int i = 0; i < 1000; i++)
            {
                store.Value.Persons.Add(new Person
                {
                    ID = i,
                    FirstName = "User",
                    LastName = "#" + 1
                });
            }

            store.Save();
        }

        [TestMethod]
        public void XmlStore_Save_DirectBatch()
        {
            var store = new XmlStore<People>(Path) { DirectWrite = true };

            for (int i = 0; i < 1000; i++)
            {
                store.Value.Persons.Add(new Person
                {
                    ID = i,
                    FirstName = "User",
                    LastName = "#" + 1
                });
            }

            store.Save();
        }

        [TestMethod]
        public void XmlStore_Save_Stress()
        {
            var store = new XmlStore<People>(Path);

            for (int i = 0; i < 100; i++)
            {
                store.Value.Persons.Add(new Person
                {
                    ID = i,
                    FirstName = "User",
                    LastName = "#" + 1
                });
                store.Save();
            }
        }

        [TestMethod]
        public void XmlStore_Save_DirectStress()
        {
            var store = new XmlStore<People>(Path) { DirectWrite = true };

            for (int i = 0; i < 100; i++)
            {
                store.Value.Persons.Add(new Person
                {
                    ID = i,
                    FirstName = "User",
                    LastName = "#" + 1
                });
                store.Save();
            }
        }

        [TestMethod]
        public void XmlStore_Read_MultiThread()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void XmlStore_Write_MultiThread()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void XmlStore_ReadWrite_MultiThread()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }

    [Serializable]
    public class Person
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime CreatedOn { get; set; } = new DateTime();
    }

    [Serializable]
    public class People
    {
        private List<Person> _persons;

        public List<Person> Persons
        {
            get { return _persons ?? (_persons = new List<Person>()); }
            set { _persons = value; }
        }
    }
}
