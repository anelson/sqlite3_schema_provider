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
    public class SchemaProviderTableTests : SchemaProviderTestsBase {
        static TableSpec[] _tableSpecs = new TableSpec[] {
            new TableSpec("simple_type_menagerie",
                new TableColumn[] {
                    new TableColumn("text_column", true, "text", DbType.String, false, false),
                    new TableColumn("numeric_column", true, "numeric", DbType.Decimal, false, false),
                    new TableColumn("integer_column", true, "integer", DbType.Int64, false, false),
                    new TableColumn("typeless_column", true, "none", DbType.Object, false, false)
                },
                new TableIndex[] {
                    new TableIndex("stm_txt", false, false, new String[] {"text_column"}),
                    new TableIndex("stm_multi", false, false, new String[] {"numeric_column", "integer_column", "typeless_column"}),
                },
                new ForeignKey[0]
            ),
            new TableSpec("int_pkey",
                new TableColumn[] {
                    new TableColumn("id", false, "integer", DbType.Int64, true, true),
                    new TableColumn("something_else", false, "text", DbType.String, false, false)
                },
                new TableIndex[] {
                    new TableIndex("sqlite_master_PK_int_pkey", true, true, new String[] {"id"}),
                    new TableIndex("ip_id", true, false, new String[] {"id"}),
                    new TableIndex("idx_ip_se", false, false, new String[] {"something_else"}),
                },
                new ForeignKey[0]
            ),
            new TableSpec("txt_pkey",
                new TableColumn[] {
                    new TableColumn("name", false, "text", DbType.String, true, true),
                    new TableColumn("something_else", false, "text", DbType.String, false, false),
                    new TableColumn("some_other_thing", false, "integer", DbType.Int64, false, false)
                },
                new TableIndex[] {
                    new TableIndex("idx_tp_se", false, false, new String[] {"something_else"}),
                    new TableIndex("sqlite_autoindex_txt_pkey_1", true, true, new String[] {"name"}),
                },
                new ForeignKey[0]
            ),
            new TableSpec("typeless_pkey",
                new TableColumn[] {
                    new TableColumn("name", false, "none", DbType.Object, true, true),
                    new TableColumn("something_else", false, "text", DbType.String, false, false),
                    new TableColumn("some_other_thing", false, "integer", DbType.Int64, false, false)
                },
                new TableIndex[] {
                    new TableIndex("idx_tlp_se", false, false, new String[] {"something_else"}),
                    new TableIndex("sqlite_autoindex_typeless_pkey_1", true, true, new String[] {"name"}),
                },
                new ForeignKey[0]
            ),
            new TableSpec("int_ai_pkey",
                new TableColumn[] {
                    new TableColumn("id", false, "integer", DbType.Int64, true, true),
                    new TableColumn("something_else", true, "text", DbType.String, false, false)
                },
                new TableIndex[] {
                    new TableIndex("sqlite_master_PK_int_ai_pkey", true, true, new String[] {"id"}),
                },
                new ForeignKey[0]
            ),
            new TableSpec("customers",
                new TableColumn[] {
                    new TableColumn("id", false, "integer", DbType.Int64, true, true),
                    new TableColumn("name", false, "text", DbType.String, false, false),
                    new TableColumn("address", false, "text", DbType.String, false, false)
                },
                new TableIndex[] {
                    new TableIndex("sqlite_master_PK_customers", true, true, new String[] {"id"}),
                },
                new ForeignKey[0]
            ),
            new TableSpec("orders",
                new TableColumn[] {
                    new TableColumn("id", false, "integer", DbType.Int64, true, true),
                    new TableColumn("customer_id", false, "integer", DbType.Int64, false, false),
                    new TableColumn("order_date", false, "text", DbType.String, false, false),
                    new TableColumn("order_total", false, "numeric", DbType.Decimal, false, false)
                },
                new TableIndex[] {
                    new TableIndex("sqlite_master_PK_orders", true, true, new String[] {"id"}),
                    new TableIndex("idx_orders_customer_id", false, false, new String[] {"customer_id"}),
                },
                new ForeignKey[] {
                    new ForeignKey("customer_id", "customers", "id")
                }
            ),
            new TableSpec("order_items",
                new TableColumn[] {
                    new TableColumn("id", false, "integer", DbType.Int64, true, true),
                    new TableColumn("order_id", false, "integer", DbType.Int64, false, false),
                    new TableColumn("description", false, "text", DbType.String, false, false),
                    new TableColumn("unit_price", false, "numeric", DbType.Decimal, false, false),
                    new TableColumn("quantity", false, "integer", DbType.Int64, false, false),
                    new TableColumn("total_price", false, "numeric", DbType.Decimal, false, false)
                },
                new TableIndex[] {
                    new TableIndex("sqlite_master_PK_order_items", true, true, new String[] {"id"}),
                    new TableIndex("idx_order_items_order_id", false, false, new String[] {"order_id"}),
                },
                new ForeignKey[] {
                    new ForeignKey("order_id", "orders", "id")
                }
            )
        };

        [Test]
        public void TableStructureTest() {
            foreach (TableSpec tbl in _tableSpecs) {
                TestTable(tbl, _db.Tables[tbl.Name]);
            }
        }

        [Test]
        public void ViewStructureTest() {
            foreach (TableSpec tbl in _tableSpecs) {
                TestView(tbl, _db.Views["v_" + tbl.Name]);
            }
        }
    }
}
