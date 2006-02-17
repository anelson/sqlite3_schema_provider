/*
 * SQLite 3 Schema Provider
 * 
 * Initial version
 * 
 * by Adam Nelson (anelson@apocryph.org)
 * 
 * This software is hereby released into the public domain.  It comes without any warranty,
 * including the implied warranties of merchantability and suitability to a particular purpose.
 * 
 * Depends upon SQLite.NET 1.0.26.3 or greater, and CodeSmith 3.2 or greater.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using SchemaExplorer;

using System.Data.SQLite;

namespace Apocryph
{
    public class Sqlite3SchemaProvider : IDbSchemaProvider
    {
        private const string Catalogs = "Catalogs";
        private const string CatalogNameColumn = "NAME";
        private const string CatalogPathColumn = "DESCRIPTION";
        private const string MainCatalog = "main";
        private const string Tables = "Tables";
        private const string TableTypeColumn = "TABLE_TYPE";
        private const string TableNameColumn = "TABLE_NAME";
        private const string TableType = "table";
        private const string ViewType = "view";
        private const string Views = "Views";
        private const string ViewsTextColumn = "VIEW_DEFINITION";
        private const string Indexes = "Indexes";
        private const string IndexesIsPkeyColumn = "PRIMARY_KEY";
        private const string IndexesNameColumn = "INDEX_NAME";
        private const string IndexesIsUniqueColumn = "UNIQUE";
        private const string IndexesIsClusteredColumn = "CLUSTERED";
        private const string IndexColumns = "IndexColumns";
        private const string IndexColumnsNameColumn = "COLUMN_NAME";
        private const string Columns = "Columns";
        private const string ColumnsNameColumn = "COLUMN_NAME";
        private const string ColumnsTypeColumn = "DATA_TYPE";
        private const string ColumnsNullableColumn = "IS_NULLABLE";
        private const string ForeignKeys = "ForeignKeys";
        private const string ForeignKeysNameColumn = "CONSTRAINT_NAME";
        private const string ForeignKeysFromTableColumn = "TABLE_NAME";
        private const string ForeignKeysFromColColumn = "FKEY_FROM_COLUMN";
        private const string ForeignKeysToTableColumn = "FKEY_TO_TABLE";
        private const string ForeignKeysToColColumn = "FKEY_TO_COLUMN";

        public Sqlite3SchemaProvider()
        {
        }

        #region IDbSchemaProvider Members

        public string Name
        {
            get { return "Sqlite3SchemaProvider"; }
        }

        public string Description
        {
            get { return "SQLite 3 Schema Provider"; }
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetCommandText(string connectionString, CommandSchema command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            //SQLite doesn't support stored commands eg stored procs
            return new CommandSchema[0];
        }

        public string GetDatabaseName(string connectionString)
        {
            return GetSchemaDataColumn(connectionString, 
                Catalogs, 
                CatalogPathColumn, 
                MainCatalog);
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            //No extended properties in SQLite
            return new ExtendedProperty[0];
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            DataTable dt = GetSchemaData(connectionString,
                Columns,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                table.Name /*restrictions[2] - table*/,
                null /*restrictions[3] - column*/);

            ColumnSchema[] columns = new ColumnSchema[dt.Rows.Count];
            int colIdx = 0;

            foreach (DataRow dr in dt.Rows)
            {
                ColumnSchema col = new ColumnSchema(table, 
                    (String)dr[ColumnsNameColumn],
                    DbTypeFromType((String)dr[ColumnsTypeColumn]),
                    NativeTypeFromType((String)dr[ColumnsTypeColumn]),
                    0,
                    0,
                    0,
                    (bool)dr[ColumnsNullableColumn]
                    );

                columns[colIdx++] = col;
            }

            return columns;
        }

        public System.Data.DataTable GetTableData(string connectionString, TableSchema table)
        {
            using (SQLiteConnection conn = GetConnection(connectionString))
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.TableDirect;
                    cmd.CommandText = table.Name;

                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;

                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    return ds.Tables[0];
                }
            }
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            DataTable dt = GetSchemaData(connectionString,
                Indexes,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                table.Name /*restrictions[2] - table*/,
                null /*restrictions[3] - unused*/,
                null /*restrictions[4] - index*/);

            IndexSchema[] indexes = new IndexSchema[dt.Rows.Count];
            int indexIdx = 0;

            foreach (DataRow dr in dt.Rows)
            {
                //Get the list of columns in this index
                DataTable cols = GetSchemaData(connectionString,
                    IndexColumns,
                    null /*restrictions[0] - catalog*/,
                    null /*restrictions[1] - unused*/,
                    table.Name /*restrictions[2] - table*/,
                    (String)dr[IndexesNameColumn] /*restrictions[3] - index*/,
                    null /*restrictions[4] - column*/);

                string[] columns = new string[cols.Rows.Count];
                int colIdx = 0;
                foreach (DataRow col in cols.Rows)
                {
                    columns[colIdx++] = (String)col[IndexColumnsNameColumn];
                }

                indexes[indexIdx++] =  new IndexSchema(table,
                    (String)dr[IndexesNameColumn],
                    dr.IsNull(IndexesIsPkeyColumn) ? false : (bool)dr[IndexesIsPkeyColumn],
                    dr.IsNull(IndexesIsUniqueColumn) ? false : (bool)dr[IndexesIsUniqueColumn],
                    dr.IsNull(IndexesIsClusteredColumn) ? false : (bool)dr[IndexesIsClusteredColumn],
                    columns);
            }

            return indexes;
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            //0, 2, 3; catalog, table, key name
            DataTable dt = GetSchemaData(connectionString,
                ForeignKeys,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                table.Name /*restrictions[2] - table*/,
                null /*restrictions[3] - key name*/);

            TableKeySchema[] keys = new TableKeySchema[dt.Rows.Count];
            int keyIdx = 0;

            foreach (DataRow dr in dt.Rows)
            {
                TableKeySchema key = new TableKeySchema(table.Database,
                    (String)dr[ForeignKeysNameColumn],
                    new string[] { (String)dr[ForeignKeysFromColColumn] },
                    (String)dr[ForeignKeysFromTableColumn],
                    new string[] { (String)dr[ForeignKeysToColColumn] },
                    (String)dr[ForeignKeysToTableColumn]);

                keys[keyIdx++] = key;
            }

            return keys;
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            DataTable dt = GetSchemaData(connectionString,
                Indexes,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                table.Name /*restrictions[2] - table*/,
                null /*restrictions[3] - unused*/,
                null /*restrictions[4] - index*/);

            //Find the primary key index
            foreach (DataRow dr in dt.Rows)
            {
                if (!dr.IsNull(IndexesIsPkeyColumn) && (bool)dr[IndexesIsPkeyColumn])
                {
                    //Get the list of columns in this primary key
                    DataTable cols = GetSchemaData(connectionString,
                        IndexColumns,
                        null /*restrictions[0] - catalog*/,
                        null /*restrictions[1] - unused*/,
                        table.Name /*restrictions[2] - table*/,
                        (String)dr[IndexesNameColumn] /*restrictions[3] - index*/,
                        null /*restrictions[4] - column*/);

                    string[] columns = new string[cols.Rows.Count];
                    int colIdx = 0;
                    foreach (DataRow col in cols.Rows)
                    {
                        columns[colIdx++] = (String)col[IndexColumnsNameColumn];
                    }

                    return new PrimaryKeySchema(table,
                        (String)dr[IndexesNameColumn],
                        columns);
                }
            }

            //No pkey
            return null;
        }

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            //Get only the 'tables' of type 'table'.  The 'Tables' catlog also includes
            //system tables, and the views

            DataTable dt = GetSchemaData(connectionString, 
                Tables, 
                null /*restrictions[0] - catalog*/, 
                null /*restrictions[1] - unused*/, 
                null /*restrictions[2] - table*/,
                TableType /*restrictions[3] - type*/);

            TableSchema[] tables = new TableSchema[dt.Rows.Count];
            int tableSchemaIdx = 0;
            foreach (DataRow dr in dt.Rows)
            {
                TableSchema table = new TableSchema(database,
                    (String)dr[TableNameColumn],
                    String.Empty,
                    DateTime.MinValue);
                tables[tableSchemaIdx++] = table;
            }

            return tables;
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            DataTable dt = GetSchemaData(connectionString,
                Columns,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                view.Name /*restrictions[2] - table*/,
                null /*restrictions[3] - column*/);

            ViewColumnSchema[] columns = new ViewColumnSchema[dt.Rows.Count];
            int colIdx = 0;

            foreach (DataRow dr in dt.Rows)
            {
                ViewColumnSchema col = new ViewColumnSchema(view,
                    (String)dr[ColumnsNameColumn],
                    DbTypeFromType((String)dr[ColumnsTypeColumn]),
                    NativeTypeFromType((String)dr[ColumnsTypeColumn]),
                    0,
                    0,
                    0,
                    (bool)dr[ColumnsNullableColumn]
                    );

                columns[colIdx++] = col;
            }

            return columns;
        }

        public System.Data.DataTable GetViewData(string connectionString, ViewSchema view)
        {

            using (SQLiteConnection conn = GetConnection(connectionString))
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.TableDirect;
                    cmd.CommandText = view.Name;

                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;

                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    return ds.Tables[0];
                }
            }
        }

        public string GetViewText(string connectionString, ViewSchema view)
        {
            DataTable dt = GetSchemaData(connectionString,
                Views,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                view.Name /*restrictions[2] - view*/);

            if (dt.Rows.Count != 1)
            {
                throw new ArgumentException(String.Format("Unexpected number of rows in Views collection for view {0}", view.Name));
            }

            return (String)dt.Rows[0][ViewsTextColumn];
        }

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            //Get only the 'tables' of type 'view'.  The 'Tables' catlog also includes
            //system tables, and regular tables

            DataTable dt = GetSchemaData(connectionString,
                Tables,
                null /*restrictions[0] - catalog*/,
                null /*restrictions[1] - unused*/,
                null /*restrictions[2] - table*/,
                ViewType /*restrictions[3] - type*/);

            ViewSchema[] views = new ViewSchema[dt.Rows.Count];
            int tableSchemaIdx = 0;
            foreach (DataRow dr in dt.Rows)
            {
                ViewSchema view = new ViewSchema(database,
                    (String)dr[TableNameColumn],
                    String.Empty,
                    DateTime.MinValue);
                views[tableSchemaIdx++] = view;
            }

            return views;
        }

        #endregion

        private SQLiteConnection GetConnection(String connectionString)
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            conn.Open();

            return conn;
        }

        private DataTable GetSchemaData(String connectionString, String schemaCollection, params string[] restrictions)
        {
            using (SQLiteConnection conn = GetConnection(connectionString))
            {
                return conn.GetSchema(schemaCollection, restrictions);
            }
        }

        private String GetSchemaDataColumn(String connectionString, String schemaCollection, String schemaColumn, params string[] restrictions)
        {
            DataTable dt = GetSchemaData(connectionString,
                schemaCollection,
                restrictions);

            if (dt.Rows.Count != 1)
            {
                throw new ArgumentException("The specified criteria do not yield a single row in the schema collection");
            }

            return (String)dt.Rows[0][schemaColumn];
        }

        private DbType DbTypeFromType(string p)
        {
            //SQLite has only a few types:
            // INTEGER == Int64
            // NUMERIC == Decimal
            // TEXT == String
            // NONE == Object or Blob
            switch (p)
            {
                case "System.Int64":
                    return DbType.Int64;

                case "System.Decimal":
                    return DbType.Decimal;

                case "System.String":
                    return DbType.String;

                default:
                    return DbType.Object;
            }                    
        }

        private string NativeTypeFromType(string p)
        {
            //SQLite has only a few types:
            // INTEGER == Int64
            // NUMERIC == Decimal
            // TEXT == String
            // NONE == Object or Blob
            switch (p)
            {
                case "System.Int64":
                    return "INTEGER";

                case "System.Decimal":
                    return "NUMERIC";

                case "System.String":
                    return "TEXT";

                default:
                    return "NONE";
            }
        }
    }
}
