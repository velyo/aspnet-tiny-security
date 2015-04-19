using System;
using System.IO;
using Artem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;

namespace Artem.Web.Security.Tests {

    /// <summary>
    ///This is a test class for PersistableTest and is intended
    ///to contain all PersistableTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PersistableTest {

        #region Static Properties /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public static string Path { get; set; }

        #endregion

        #region Static Methods ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Classes the initialize.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext) {
            Path = string.Format(@"{0}\Test.xml", testContext.TestDir);
        }
        #endregion

        #region Initialize


        #region Properties  ///////////////////////////////////////////////////////////////////////

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return _testContext;
            }
            set {
                _testContext = value;
            }
        }
        private TestContext _testContext;
        #endregion

        #region Additional test attributes
        // 

        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        #endregion

        #region Methods ///////////////////////////////////////////////////////////////////////////

        [TestMethod]
        public void CollectionPerformanceTest() {

            var p = new Persistable<People>(Path);
            Stopwatch watch = new Stopwatch();

            watch.Start();
            for (int i = 0; i < 1000; i++) {
                p.Value.Persons.Add(new Person {
                    FirstName = "Velio",
                    LastName = "Ivanov",
                    Age = 40
                });
                p.Save();
            }
            watch.Stop();

            Console.Out.WriteLine(string.Format("Time elapsed: {0}", watch.Elapsed));
        }

        [TestMethod()]
        public void DeleteTest() {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void ReadThreadingTest() {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void WriteThreadingTest() {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void ReadWriteThreadingTest() {
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
        #endregion
    }

    public class Person {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class People {

        public List<Person> Persons {
            get { return _persons ?? (_persons = new List<Person>()); }
            set { _persons = value; }
        }
        List<Person> _persons;
    }
}
