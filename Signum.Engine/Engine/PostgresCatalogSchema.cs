using Signum.Engine.Maps;
using Signum.Engine.PostgresCatalog;
using Signum.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NpgsqlTypes;
using static Signum.Engine.PostgresCatalog.PostgresFunctions;

namespace Signum.Engine.Engine
{
    public static class PostgresCatalogSchema
    {
        static string[] systemSchemas = new[] { "pg_catalog", "pg_toast", "information_schema" };

        public static Dictionary<string, DiffTable> GetDatabaseDescription(List<DatabaseName?> databases)
        {
            List<DiffTable> allTables = new List<DiffTable>();

            var isPostgres = Schema.Current.Settings.IsPostgres;

            foreach (var db in databases)
            {
                SafeConsole.WriteColor(ConsoleColor.Cyan, '.');

                using (Administrator.OverrideDatabaseInSysViews(db))
                {
                    var databaseName = db == null ? Connector.Current.DatabaseName() : db.Name;

                    //var sysDb = Database.View<SysDatabases>().Single(a => a.name == databaseName);

                    var con = Connector.Current;

                    var tables =
                        (from s in Database.View<PgNamespace>()
                         where !systemSchemas.Contains(s.nspname)
                         from t in s.Tables()
                         select new DiffTable
                         {
                             Name = new ObjectName(new SchemaName(db, s.nspname, isPostgres), t.relname, isPostgres),

                             TemporalType = !con.SupportsTemporalTables ? Signum.Engine.SysTableTemporalType.None : t.Triggers().Any(t => t.Proc().proname.StartsWith("versioning_function")) ? Signum.Engine.SysTableTemporalType.SystemVersionTemporalTable : SysTableTemporalType.None,

                             //Period = !con.SupportsTemporalTables ? null :

                             //(from p in t.Periods()
                             // join sc in t.Columns() on p.start_column_id equals sc.column_id
                             // join ec in t.Columns() on p.end_column_id equals ec.column_id
                             // select new DiffPeriod
                             // {
                             //     StartColumnName = sc.name,
                             //     EndColumnName = ec.name,
                             // }).SingleOrDefaultEx(),

                             //TemporalTableName = !con.SupportsTemporalTables || t.history_table_id == null ? null :
                             //    Database.View<SysTables>()
                             //    .Where(ht => ht.object_id == t.history_table_id)
                             //    .Select(ht => new ObjectName(new SchemaName(db, ht.Schema().name, isPostgres), ht.name, isPostgres))
                             //    .SingleOrDefault(),

                             PrimaryKeyName = (from ind in t.Indices()
                                               where ind.indisprimary == true
#pragma warning disable CS0472
                                               select ((int?)ind.indexrelid) == null ? null : new ObjectName(new SchemaName(db, ind.Class().Namespace().nspname, isPostgres), ind.Class().relname, isPostgres))
#pragma warning restore CS0472
                                               .SingleOrDefaultEx(),

                             Columns = (from c in t.Attributes()
                                            //join userType in Database.View<SysTypes>().DefaultIfEmpty() on c.user_type_id equals userType.user_type_id
                                            //join sysType in Database.View<SysTypes>().DefaultIfEmpty() on c.system_type_id equals sysType.user_type_id
                                            //join ctr in Database.View<SysDefaultConstraints>().DefaultIfEmpty() on c.default_object_id equals ctr.object_id
                                        select new DiffColumn
                                        {
                                            Name = c.attname,
                                            DbType = new AbstractDbType(ToNpgsqlDbType(c.Type().typname)),
                                            UserTypeName = null,
                                            Nullable = !c.attnotnull,
                                            Collation = null,
                                            Length = c.attlen,
                                            Precision = c.atttypid == 1700  /*numeric*/ ? ((c.atttypmod - 4) >> 16) & 65535 : 0,
                                            Scale = c.atttypid == 1700  /*numeric*/ ? (c.atttypmod - 4) & 65535 : 0,
                                            Identity = c.attidentity == 'a',
                                            GeneratedAlwaysType = GeneratedAlwaysType.None,
                                            DefaultConstraint = c.Default() == null ? null : new DiffDefaultConstraint
                                            {
                                                Definition = pg_get_expr(c.Default()!.adbin, c.Default()!.adrelid),
                                            },
                                            PrimaryKey = t.Indices().Any(i => i.indisprimary && i.indkey.Contains(c.attnum)),
                                        }).ToDictionaryEx(a => a.Name, "columns"),

                             MultiForeignKeys = (from fk in t.Constraints()
                                                 where fk.contype == ConstraintType.ForeignKey
                                                 select new DiffForeignKey
                                                 {
                                                     Name = new ObjectName(new SchemaName(db, fk.Namespace().nspname, isPostgres), fk.conname, isPostgres),
                                                     IsDisabled = false,
                                                     TargetTable = new ObjectName(new SchemaName(db, fk.TargetTable().Namespace().nspname, isPostgres), fk.TargetTable().relname, isPostgres),
                                                     Columns = PostgresFunctions.generate_subscripts(fk.conkey, 0).Select(i => new DiffForeignKeyColumn
                                                     {
                                                         Parent = t.Attributes().Single(c => c.attnum == fk.conkey[i]).attname,
                                                         Referenced = fk.TargetTable().Attributes().Single(c => c.attnum == fk.confkey[i]).attname,
                                                     }).ToList(),
                                                 }).ToList(),

                             SimpleIndices = (from i in t.Indices()
                                              where !i.indisprimary
                                              select new DiffIndex
                                              {
                                                  IsUnique = i.indisunique,
                                                  IsPrimary = i.indisprimary,
                                                  IndexName = i.Class().relname,
                                                  FilterDefinition = PostgresFunctions.pg_get_expr(i.indpred!, i.indrelid),
                                                  Type = DiffIndexType.NonClustered,
                                                  Columns = (from at in i.Class().Attributes()
                                                             orderby at.attnum
                                                             select new DiffIndexColumn { ColumnName = at.attname, IsIncluded = !i.indkey.Contains(at.attnum) }).ToList()
                                              }).ToList(),

                             ViewIndices = new List<DiffIndex>(),

                             Stats = new List<DiffStats>(),

                         }).ToList();


                    if (SchemaSynchronizer.IgnoreTable != null)
                        tables.RemoveAll(SchemaSynchronizer.IgnoreTable);

                    tables.ForEach(t => t.Columns.RemoveAll(c => c.Value.DbType.PostgreSql == (NpgsqlDbType)(-1)));

                    tables.ForEach(t => t.ForeignKeysToColumns());

                    allTables.AddRange(tables);
                }
            }

            var database = allTables.ToDictionary(t => t.Name.ToString());

            return database;
        }


        public static NpgsqlDbType ToNpgsqlDbType(string str)
        {
            switch (str)
            {
                case "bool": return NpgsqlDbType.Boolean;
                case "bytea": return NpgsqlDbType.Bytea;
                case "char": return NpgsqlDbType.Char;
                case "int8": return NpgsqlDbType.Bigint;
                case "int2": return NpgsqlDbType.Smallint;
                case "float4": return NpgsqlDbType.Real;
                case "float8": return NpgsqlDbType.Double;
                case "int2vector": return NpgsqlDbType.Smallint | NpgsqlDbType.Array;
                case "int4": return NpgsqlDbType.Integer;
                case "text": return NpgsqlDbType.Text;
                case "json": return NpgsqlDbType.Json;
                case "xml": return NpgsqlDbType.Xml;
                case "point": return NpgsqlDbType.Point;
                case "lseg": return NpgsqlDbType.LSeg;
                case "path": return NpgsqlDbType.Path;
                case "box": return NpgsqlDbType.Box;
                case "polygon": return NpgsqlDbType.Polygon;
                case "line": return NpgsqlDbType.Line;
                case "circle": return NpgsqlDbType.Circle;
                case "money": return NpgsqlDbType.Money;
                case "macaddr": return NpgsqlDbType.MacAddr;
                case "macaddr8": return NpgsqlDbType.MacAddr8;
                case "inet": return NpgsqlDbType.Inet;
                case "varchar": return NpgsqlDbType.Varchar;
                case "date": return NpgsqlDbType.Date;
                case "time": return NpgsqlDbType.Time;
                case "timestamp": return NpgsqlDbType.Timestamp;
                case "timestamptz": return NpgsqlDbType.TimestampTz;
                case "interval": return NpgsqlDbType.Interval;
                case "timetz": return NpgsqlDbType.TimestampTz;
                case "bit": return NpgsqlDbType.Bit;
                case "varbit": return NpgsqlDbType.Varbit;
                case "numeric": return NpgsqlDbType.Numeric;
                case "uuid": return NpgsqlDbType.Uuid;
                case "tsvector": return NpgsqlDbType.TsVector;
                case "tsquery": return NpgsqlDbType.TsQuery;
                case "jsonb": return NpgsqlDbType.Jsonb;
                case "int4range": return NpgsqlDbType.Range | NpgsqlDbType.Integer;
                case "numrange": return NpgsqlDbType.Range | NpgsqlDbType.Numeric;
                case "tsrange": return NpgsqlDbType.Range | NpgsqlDbType.Timestamp;
                case "tstzrange": return NpgsqlDbType.Range | NpgsqlDbType.TimestampTz;
                case "daterange": return NpgsqlDbType.Range | NpgsqlDbType.Date;
                case "int8range": return NpgsqlDbType.Range | NpgsqlDbType.Bigint;
                case "oid":
                case "cid":
                case "xid":
                case "tid":
                    return (NpgsqlDbType)(-1);
                default: 
                    return (NpgsqlDbType)(-1);
            }

        }

        public static HashSet<SchemaName> GetSchemaNames(List<DatabaseName?> list)
        {
            var sqlBuilder = Connector.Current.SqlBuilder;
            var isPostgres = sqlBuilder.IsPostgres;
            HashSet<SchemaName> result = new HashSet<SchemaName>();
            foreach (var db in list)
            {
                using (Administrator.OverrideDatabaseInSysViews(db))
                {
                    var schemaNames = Database.View<PgNamespace>().Select(s => s.nspname).ToList();

                    result.AddRange(schemaNames.Select(sn => new SchemaName(db, sn, isPostgres)).Where(a => !SchemaSynchronizer.IgnoreSchema(a)));
                }
            }
            return result;
        }
    }
}
