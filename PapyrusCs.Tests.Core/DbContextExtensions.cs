using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FastMember;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public static class DbContextExtensions
    {

        public static void ReadObject<T>(IEnumerable<T> entities)
        {
            var a = FastMember.ObjectReader.Create<T>(entities);

            Stopwatch sp = Stopwatch.StartNew();
            while (a.Read())
            {
                for (int i = 0; i < a.FieldCount; i++)
                {
                    object temp = a[i];
                }
            }

            Console.WriteLine(sp.Elapsed);
            Console.WriteLine(entities.Count());
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> entities)
        {
            var table = GetTableName(dbContext, typeof(T));
            var (keys, columns) = GetColumns(dbContext, typeof(T));

            var columnCount = columns.Count();
            var entityCount = entities.Count();
            var reader = ObjectReader.Create(entities, columns.Select(x => x.Property).ToArray());

            var (parameters, batchSize, remainder) = GetParametersAndBatchSize(dbContext, columns.Count(), entityCount);

            var indexInBatch = 0;
            while (reader.Read()) // per Row
            {
                for (int i = 0; i < columnCount; i++)
                {
                    parameters[indexInBatch * batchSize + i].Value = reader[i];
                }
                indexInBatch++;

                if (indexInBatch == batchSize)
                {
                    var sql = GetSqlInsertString(table, columns.Select(x => x.DbColumn), batchSize);
                }
            }
        }

        public static string GetSqlInsertString(string table, IEnumerable<string> columns, int batchSize = 1)
        {
            var columnCount = columns.Count();

            var sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} ({1}) VALUES ", table, string.Join(", ", columns));

            var counter = 0;
            for (int b = 0; b < batchSize; b++)
            {
                sb.AppendFormat("({0})", string.Join(",", Enumerable.Range(counter, columnCount).Select(x => "@p" + x)));
                if (b < batchSize - 1)
                {
                    sb.Append(",");
                }
                counter += columnCount;
            }

            return sb.ToString();
        }

        public static (DbParameter[], int, int) GetParametersAndBatchSize(DbContext dbContext, int parameterPerRow, int entityCount)
        {
            int maxParametersPerInsert = 999;
            int batchSize = maxParametersPerInsert / parameterPerRow;
            int remainderBatchSize = 0;

            if (batchSize > entityCount)
            {
                batchSize = entityCount;
                remainderBatchSize = 0;
            }
            else
            {
                remainderBatchSize = entityCount % batchSize;
            }

            var con = dbContext.Database.GetDbConnection();
            var cmd = con.CreateCommand();
            DbParameter[] parameter = Enumerable.Range(0, batchSize*parameterPerRow).Select(x =>
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "p" + x;
                return p;
            }).ToArray();

            return (parameter, batchSize, remainderBatchSize);

        }

        /// <summary>
        /// Retrieves the table name of entitiy T in context dbContext
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="t"></param>
        /// <returns>name of the table in the database</returns>
        public static string GetTableName(DbContext dbContext, Type t)
        {
            var mapping = dbContext.Model.FindEntityType(t).Relational();
            var schema = mapping.Schema;
            return mapping.TableName;
        }

        public struct PropertyDbName
        {
            public PropertyDbName(string property, string dbColumn)
            {
                Property = property;
                DbColumn = dbColumn;
            }
            public string Property { get; }
            public string DbColumn { get; }
        }

        public static (IEnumerable<PropertyDbName>, IEnumerable<PropertyDbName>) GetColumns(DbContext dbContext, Type t)
        {
            var keyRet = new List<PropertyDbName>();
            var propRet = new List<PropertyDbName>();

            var et = dbContext.Model.FindEntityType(t);
            foreach (var c in et.GetProperties())
            {
                if (c.IsKey())
                {
                    keyRet.Add(new PropertyDbName(c.Name, c.Relational().ColumnName));
                }
                else
                {
                    propRet.Add(new PropertyDbName(c.Name, c.Relational().ColumnName));
                }

            }

            return (keyRet, propRet);
        }
    }
}