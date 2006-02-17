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
    internal struct TableColumn {
        public string Name;
        public bool Nullable;
        public string NativeType;
        public DbType DatabaseType;
        public bool InPrimaryKey;
        public bool IsUnique;

        public TableColumn(string name, bool nullable, string nativeType, DbType dbType, bool inPkey, bool unique) {
            Name = name;
            Nullable = nullable;
            NativeType = nativeType;
            DatabaseType = dbType;
            InPrimaryKey = inPkey;
            IsUnique = unique;
        }
    }

    internal struct TableIndex {
        public string Name;
        public bool IsUnique;
        public bool IsPrimaryKey;
        public String[] Columns;

        public TableIndex(string name, bool isUnique, bool isPrimaryKey, String[] columns) {
            Name = name;
            IsUnique = isUnique;
            IsPrimaryKey = isPrimaryKey;
            Columns = columns;
        }
    }

    internal struct ForeignKey {
        public string FromColumn;
        public string ToTable;
        public string ToColumn;

        public ForeignKey(String fromCol, String toTable, String toCol) {
            FromColumn = fromCol;
            ToTable = toTable;
            ToColumn = toCol;
        }
    }

    internal struct TableSpec {
        public String Name;
        public TableColumn[] Columns;
        public TableIndex[] Indexes;
        public ForeignKey[] ForeignKeys;

        public TableSpec(String name, TableColumn[] columns, TableIndex[] indexes, ForeignKey[] fkeys) {
            Name = name;
            Columns = columns;
            Indexes = indexes;
            ForeignKeys = fkeys;
        }
    }

    public class SchemaProviderTestsBase
    {
        protected String _dbPath;
        protected DatabaseSchema _db;


        [SetUp]
        public void CreateSchemaObject() {
            _db = new DatabaseSchema();
            _db.Provider = new Sqlite3SchemaProvider();
            _db.ConnectionString = String.Format("Data Source={0};", _dbPath);
        }

        [TestFixtureSetUp]
        public void CreateTestDb()
        {
            //Create a new SQLite database with the test schema
            _dbPath = Path.GetTempFileName();

            using (SQLiteConnection conn = new SQLiteConnection())
            {
                SQLiteConnection.CreateFile(_dbPath);
                conn.ConnectionString = String.Format("Data Source={0}", _dbPath);
                conn.Open();

                String dbSchemaScript;

                using (TextReader tr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Apocryph.Tests.TestDb.sql")))
                {
                    dbSchemaScript = tr.ReadToEnd();
                }

                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = dbSchemaScript;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [TestFixtureTearDown]
        public void ClearDatabase()
        {
            File.Delete(_dbPath);
        }

        internal void TestTable(TableSpec spec, TableSchema tbl) {
            String tableName = spec.Name;

            Assert.IsNotNull(tbl);

            Assert.AreEqual(tableName, tbl.Name);
            Assert.AreEqual(spec.Columns.Length,
                tbl.Columns.Count);

            for (int idx = 0; idx < tbl.Columns.Count; idx++) {
                CompareColumns(spec.Columns[idx], tbl.Columns[idx]);
            }

            Assert.AreEqual(spec.Indexes.Length,
                tbl.Indexes.Count);

            for (int idx = 0; idx < tbl.Indexes.Count; idx++) {
                CompareIndexes(spec.Indexes[idx], tbl.Indexes[spec.Indexes[idx].Name]);
            }

            Assert.AreEqual(spec.ForeignKeys.Length,
                tbl.Keys.Count);

            for (int idx = 0; idx < tbl.Keys.Count; idx++) {
                CompareForeignKeys(spec.ForeignKeys[idx], tbl.Keys[idx]);
            }
        }

        internal void TestView(TableSpec spec, ViewSchema tbl) {
            String viewName = "v_" + spec.Name;

            Assert.IsNotNull(tbl);

            Assert.AreEqual(viewName, tbl.Name);
            Assert.AreEqual(spec.Columns.Length,
                tbl.Columns.Count);

            for (int idx = 0; idx < tbl.Columns.Count; idx++) {
                CompareColumns(spec.Columns[idx], tbl.Columns[idx]);
            }
        }

        private void CompareColumns(TableColumn colSpec, ColumnSchema col) {
            Assert.AreEqual(colSpec.Name, col.Name);
            Assert.AreEqual(colSpec.NativeType.ToLower(), col.NativeType.ToLower());
            Assert.AreEqual(colSpec.DatabaseType, col.DataType);
            Assert.AreEqual(colSpec.InPrimaryKey, col.IsPrimaryKeyMember);
            Assert.AreEqual(colSpec.IsUnique, col.IsUnique);
            Assert.AreEqual(0, col.Scale);
            Assert.AreEqual(0, col.Precision);
            Assert.AreEqual(0, col.Size);
        }

        private void CompareColumns(TableColumn colSpec, ViewColumnSchema col) {
            Assert.AreEqual(colSpec.Name, col.Name);
            Assert.AreEqual(colSpec.NativeType.ToLower(), col.NativeType.ToLower());
            Assert.AreEqual(colSpec.DatabaseType, col.DataType);
            Assert.AreEqual(0, col.Scale);
            Assert.AreEqual(0, col.Precision);
            Assert.AreEqual(0, col.Size);
        }

        private void CompareIndexes(TableIndex tableIndex, IndexSchema indexSchema) {
            Assert.AreEqual(tableIndex.Name, indexSchema.Name);
            Assert.AreEqual(tableIndex.IsPrimaryKey, indexSchema.IsPrimaryKey);
            Assert.AreEqual(tableIndex.IsUnique, indexSchema.IsUnique);
            Assert.AreEqual(false, indexSchema.IsClustered);
            Assert.AreEqual(tableIndex.Columns.Length, indexSchema.MemberColumns.Count);

            for (int idx = 0; idx < tableIndex.Columns.Length; idx++) {
                Assert.AreEqual(tableIndex.Columns[idx],
                    indexSchema.MemberColumns[idx].Name);
            }
        }

        private void CompareForeignKeys(ForeignKey foreignKey, TableKeySchema tableKeySchema) {
            Assert.AreEqual(1, tableKeySchema.ForeignKeyMemberColumns.Count);
            Assert.AreEqual(foreignKey.FromColumn,
                tableKeySchema.ForeignKeyMemberColumns[0].Name);
            Assert.AreEqual(foreignKey.ToTable,
                tableKeySchema.PrimaryKeyTable.Name);
            Assert.AreEqual(1, tableKeySchema.PrimaryKeyMemberColumns.Count);
            Assert.AreEqual(foreignKey.ToColumn,
                tableKeySchema.PrimaryKeyMemberColumns[0].Name);
        }
    }
}
