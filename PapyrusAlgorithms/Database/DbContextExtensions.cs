using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FastMember;
using Microsoft.EntityFrameworkCore;

namespace PapyrusCs.Database
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

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> entities)
        {
            var table = GetTableName(dbContext, typeof(T));
            var (keys, columns) = GetColumns(dbContext, typeof(T));

            var keyList = keys.ToList();
            var columnList = columns.ToList();
            var entityList = entities.ToList();

            var keyCount = keyList.Count();
            var columnCount = columnList.Count();
            var entityCount = entityList.Count();
            var reader = ObjectReader.Create(entityList, keyList.Union(columnList).Select(x => x.Property).ToArray());

            var (parameters, _, _) = GetUpdateParametersAndBatchSize(dbContext, keyCount, columnCount, entityCount);

            dbContext.Database.BeginTransaction();
            var sql = GetSqlUpdateString(table, keyList.Select(x => x.DbColumn), columnList.Select(x => x.DbColumn), 1);

            while (reader.Read()) // per Row
            {
                for (int i = 0; i < keyCount; i++)
                {
                    parameters[i].Value = reader[i];
                }

                for (int i = 0; i < columnCount; i++)
                {
                    var value = reader[keyCount + i];
                    parameters[keyCount + i].Value = value;
                }

                dbContext.Database.ExecuteSqlRaw(sql, parameters);
            }

            dbContext.Database.CommitTransaction();
        }


        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> entities)
        {
            var table = GetTableName(dbContext, typeof(T));
            var (keys, columns) = GetColumns(dbContext, typeof(T));

            var columnList = columns.ToList();
            var entityList = entities.ToList();

            var columnCount = columnList.Count();
            var entityCount = entityList.Count();
            var reader = ObjectReader.Create(entityList, columnList.Select(x => x.Property).ToArray());

            var (parameters, batchSize, remainderBatch) = GetInsertParametersAndBatchSize(dbContext, columnCount, entityCount);

            var indexInBatch = 0;
            dbContext.Database.BeginTransaction();
            var sql = GetSqlInsertString(table, columnList.Select(x => x.DbColumn), batchSize);

            while (reader.Read()) // per Row
            {
                for (int i = 0; i < columnCount; i++)
                {
                    parameters[indexInBatch* columnCount + i].Value = reader[i];
                }
                indexInBatch++;

                if (indexInBatch == batchSize)
                {
                    dbContext.Database.ExecuteSqlRaw(sql, parameters);
                    indexInBatch = 0;
                }
            }

            if (indexInBatch > 0)
            {
                var finalSql = GetSqlInsertString(table, columnList.Select(x => x.DbColumn), remainderBatch);
                var paras = parameters.Take(columnCount * remainderBatch);
                dbContext.Database.ExecuteSqlRaw(finalSql, paras);
            }

            try
            {
                dbContext.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static string GetSqlUpdateString(string table, IEnumerable<string> keys, IEnumerable<string> columns, int batchSize = 1)
        {
            var columnList = columns.ToList();
            var columnCount = columnList.Count();
            var keyList = keys.ToList();
            var keyCount = keyList.Count();
            if (batchSize != 1)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            var sb = new StringBuilder();
            sb.AppendFormat("UPDATE {0} SET ", table);

            for (int i = 0; i < columnCount; i++)
            {
                sb.AppendFormat("'{0}'=@p{1}", columnList[i], i);
                if (i < columnCount - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(" WHERE (");


            sb.Append(string.Join(" AND ", Enumerable.Range(0, keyCount).Select(x => $"{keyList[x]}=@k{x}")));

            sb.Append(");");
            return sb.ToString();
        }


        public static string GetSqlInsertString(string table, IEnumerable<string> columns, int batchSize = 1)
        {
            var columnList = columns.ToList();
            var columnCount = columnList.Count();

            var sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} ({1}) VALUES ", table, string.Join(", ", columnList));

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

        public static (DbParameter[], int, int) GetUpdateParametersAndBatchSize(DbContext dbContext, int keysPerRow, int parameterPerRow, int entityCount)
        {
            using (var con = dbContext.Database.GetDbConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    DbParameter[] parameter =
                        Enumerable.Range(0, keysPerRow).Select(x =>
                            {
                                var p = cmd.CreateParameter();
                                p.ParameterName = "k" + x;
                                return p;
                            })
                            .Union(
                                Enumerable.Range(0, parameterPerRow).Select(x =>
                                {
                                    var p = cmd.CreateParameter();
                                    p.ParameterName = "p" + x;
                                    return p;
                                }))
                            .ToArray();

                    return (parameter, 1, 0);
                }
            }
        }

        public static (DbParameter[], int, int) GetInsertParametersAndBatchSize(DbContext dbContext, int parameterPerRow, int entityCount)
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

            using (var con = dbContext.Database.GetDbConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    DbParameter[] parameter = Enumerable.Range(0, batchSize * parameterPerRow).Select(x =>
                    {
                        var p = cmd.CreateParameter();
                        p.ParameterName = "p" + x;
                        return p;
                    }).ToArray();

                    return (parameter, batchSize, remainderBatchSize);
                }
            }
        }

        /// <summary>
        /// Retrieves the table name of entitiy T in context dbContext
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="t"></param>
        /// <returns>name of the table in the database</returns>
        public static string GetTableName(DbContext dbContext, Type t)
        {
            return dbContext.Model.FindEntityType(t).GetTableName();
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
                    keyRet.Add(new PropertyDbName(c.Name, c.GetColumnName()));
                }
                else
                {
                    propRet.Add(new PropertyDbName(c.Name, c.GetColumnName()));
                }

            }

            return (keyRet, propRet);
        }
    }
}
