using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Velyo.Web.Security;

namespace Artem.Web.Security.Tests
{
    /// <summary>
    ///This is a test class for PersistableTest and is intended
    ///to contain all PersistableTest Unit Tests
    ///</summary>
    [TestClass]
    public class XmlStoreTest
    {
        private static string Path { get; set; }


        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Path = string.Format(@"{0}\Test.xml", testContext.TestDir);
        }


        [TestMethod]
        public void DeleteTest()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void LoadTest()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void SaveBatchTest()
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
        public void SaveDirectBatchTest()
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
        public void SaveStressTest()
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
        public void SaveDirectStressTest()
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
        public void ReadThreadingTest()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void WriteThreadingTest()
        {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void ReadWriteThreadingTest()
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
