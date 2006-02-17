using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;

using NUnit.Framework;
using SchemaExplorer;

using Apocryph;

namespace Apocryph.Tests {
    [TestFixture]
    public class SchemaProviderBasicTests : SchemaProviderTestsBase {

        [Test]
        public void ProviderNameTest() {
            Assert.AreEqual("Sqlite3SchemaProvider",
                _db.Provider.Name);
        }

        [Test]
        public void ProviderDescTest() {
            Assert.AreEqual("SQLite 3 Schema Provider",
                _db.Provider.Description);
        }

        [Test]
        public void DbNameTest() {
            Assert.AreEqual(_dbPath, _db.Name);
        }

        [Test]
        public void NoCommandsTest() {
            Assert.AreEqual(0, _db.Commands.Count);
        }

        [Test]
        public void TableCountTest() {
            Assert.AreEqual(8, _db.Tables.Count);
        }

        [Test]
        public void ViewCountTest() {
            Assert.AreEqual(6, _db.Views.Count);
        }
    }
}
