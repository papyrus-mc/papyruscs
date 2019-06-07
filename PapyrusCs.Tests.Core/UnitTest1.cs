using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PapyrusCs.Database;

namespace Tests
{
    public class MyMiniC
    {
        public int Id { get; set; }
        public uint Crc42 { get; set; }
    }


    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MyMiniC> MyMinis { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }

    }


    public class Tests
    {
        public (IEnumerable<string>, IEnumerable<string>) GetKeysAndProperties(DbContext context, Type type)
        {
            var keys = new List<string>();
            var properties = new List<string>();

            var entityType = context.Model.FindEntityType(type);
            var nonkeys = entityType.GetProperties();
            foreach (var key in nonkeys)
            {
                if (key.IsKey())
                {
                    keys.Add(key.Name);
                }
                else
                {
                    properties.Add(key.Name);
                }
            }

            return (keys, properties);
        }



        [Test]
        public void TestFastMember()
        {
            var c = FastMember.ObjectAccessor.Create(new MyMiniC());


        }

        [TestCase(3, 5, 15, 5, 0)]
        [TestCase(2,1500,998,499,3)]
        [TestCase(7, 50000, 994, 142, 16)]
        public void ParameterTest(int columns, int rows, int expectedParameterCount, int expectedBatchSize, int expectedRemainderBatchsize)
        {
            DbContextOptionsBuilder<MyContext> opt = new DbContextOptionsBuilder<MyContext>();
            opt.UseSqlite("Filename=testparameter.sqlite");

            var context = new MyContext(opt.Options);

            var (dbParameters, batchSize, remainder) = DbContextExtensions.GetParametersAndBatchSize(context, columns, rows);

            Assert.That(dbParameters.Length, Is.EqualTo(expectedParameterCount));
            Assert.That(batchSize, Is.EqualTo(expectedBatchSize));
            Assert.That(remainder, Is.EqualTo(expectedRemainderBatchsize));
            Assert.That(dbParameters[0].ParameterName, Is.EqualTo("p0"));
            Assert.That(dbParameters[dbParameters.Length-1].ParameterName, Is.EqualTo("p"+(dbParameters.Length-1)));
        }

        [Test]
        public void TestSql()
        {
            var dut = DbContextExtensions.GetSqlInsertString("lala", new string[] {"a", "b", "c"}, 5);
            Assert.That(dut.Contains("@p0"));
            Assert.That(dut.Contains("@p14"));
            Assert.That(dut.Count(x => x == '@'), Is.EqualTo(15));
            Console.WriteLine(dut);
        }

        [Test]
        public void RawSqlTest()
        {
            DbContextOptionsBuilder<MyContext> opt = new DbContextOptionsBuilder<MyContext>();
            //opt.UseInMemoryDatabase("testdb");
            opt.UseSqlite("Filename=test.sqlite");

            var context = new MyContext(opt.Options);
            context.Database.EnsureCreated();

            int number = 17;
            context.Database.ExecuteSqlRaw($"INSERT INTO MyMinis ('Crc42') VALUES (@p1)", new object[] {new SqliteParameter("p1", number)});

            var dbCOnnection = context.Database.GetDbConnection();
            var cmd = dbCOnnection.CreateCommand();
            dbCOnnection.Open();
            cmd.CommandText = "INSERT INTO MyMinis('Crc42') VALUES(@p1)";
            var p = cmd.CreateParameter();
            p.ParameterName = "p1";
            p.Value = number + 1;
            cmd.Parameters.Add(p);
            cmd.ExecuteNonQuery();
            dbCOnnection.Close();

            context.Database.ExecuteSqlRaw($"INSERT INTO MyMinis ('Crc42') VALUES (@p1)", new object[] { p });

            var result = context.MyMinis.ToList();
            foreach (var myMiniC in result)
            {
                Console.WriteLine(myMiniC.Crc42);
            }
        }

        [Test]
        public void BulkTest()
        {
            DbContextOptionsBuilder<MyContext> opt = new DbContextOptionsBuilder<MyContext>();
            opt.UseInMemoryDatabase("testdb");

            var context = new MyContext(opt.Options);
            var (keys, properties) = GetKeysAndProperties(context, typeof(MyMiniC));

            foreach (var key in keys)
            {
                Console.WriteLine($"Key: {key}");
            }

            foreach (var property in properties)
            {
                Console.WriteLine($"Property: {property}");
            }

        }

        public (string, object[] parameters) CreateInsert(string table, IEnumerable<Tuple<string, object>> properties)
        {
            string baseString = "INSERT INTO '{0}' ({1}) VALUES ({2})";

            var parNames = string.Join(",", properties.Select(x => $"'{x.Item1}'"));
            //var parValues = 

            return (null, null);
        }


        
    }
}
