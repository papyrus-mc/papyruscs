using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PapyrusAlgorithms.Database;
using PapyrusCs.Database;
using FluentAssertions;

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


    public class 
    Tests
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



        [Fact]
        public void TestBulkInsert()
        {
            DbContextOptionsBuilder<MyContext> opt = new DbContextOptionsBuilder<MyContext>();
            opt.UseSqlite("Filename=testparameter.sqlite");
            
            var context = new MyContext(opt.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var duts = Enumerable.Range(0, 100000).Select(x => new MyMiniC() {Crc42 = (uint) x});

            var sp = Stopwatch.StartNew();
            context.BulkInsert(duts);
            Console.WriteLine(sp.Elapsed);
        }



        [Fact]
        public void TestBulkInsertAndUpdate()
        {
            DbContextOptionsBuilder<MyContext> opt = new DbContextOptionsBuilder<MyContext>();
            opt.UseSqlite("Filename=testparameter2.sqlite");

            var context = new MyContext(opt.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var duts = Enumerable.Range(0, 100000).Select(x => new MyMiniC() { Crc42 = (uint)x });

            var sp = Stopwatch.StartNew();
            context.BulkInsert(duts);
            Console.WriteLine(sp.Elapsed);

            var newduts = context.MyMinis;
            newduts.Count().Should().Be(100000);
            foreach (var myMiniC in newduts)
            {
                myMiniC.Crc42 += 10000;
            }

            sp = Stopwatch.StartNew();
            context.BulkUpdate(newduts);
            Console.WriteLine(sp.Elapsed);
        }

        [Theory]
        [InlineData(3, 5, 15, 5, 0)]
        [InlineData(2,1500,998,499,3)]
        [InlineData(7, 50000, 994, 142, 16)]
        public void ParameterTest(int columns, int rows, int expectedParameterCount, int expectedBatchSize, int expectedRemainderBatchsize)
        {
            DbContextOptionsBuilder<MyContext> opt = new DbContextOptionsBuilder<MyContext>();
            opt.UseSqlite("Filename=testparameter.sqlite");

            var context = new MyContext(opt.Options);

            var (dbParameters, batchSize, remainder) = DbContextExtensions.GetInsertParametersAndBatchSize(context, columns, rows);

            dbParameters.Length.Should().Be(expectedParameterCount);
            batchSize.Should().Be(expectedBatchSize);
            remainder.Should().Be(expectedRemainderBatchsize);
            dbParameters[0].ParameterName.Should().Be("p0");
            dbParameters[dbParameters.Length-1].ParameterName.Should().Be("p" +(dbParameters.Length-1));
        }

        [Fact]
        public void TestInsertSql()
        {
            var dut = DbContextExtensions.GetSqlInsertString("lala", new string[] {"a", "b", "c"}, 5);
            dut.Should().Contain("@p0");
            dut.Should().Contain("@p14");
            dut.Count(x => x == '@').Should().Be(15);
            Console.WriteLine(dut);
        }

        [Fact]
        public void TestUpdateSql()
        {
            
            var dut = DbContextExtensions.GetSqlUpdateString("lala", new []{"id"},  new [] { "a", "b", "c" }, 1);
            dut.Should().Contain("@p0");
            dut.Should().Contain("@p2");
            dut.Should().Contain("@k0");
            dut.Count(x => x == '@').Should().Be(4);
            Console.WriteLine(dut);
        }

        [Fact]
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

        [Fact]
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

        
    }
}
