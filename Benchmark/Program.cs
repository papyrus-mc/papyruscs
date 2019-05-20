using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using Microsoft.DotNet.InternalAbstractions;
using PapyrusCs.Database;

namespace Benchmark
{
    //[CoreJob(), RyuJitX64Job]
    //[Config(typeof(Config))]
    public class Benchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Core.With(CsProjCoreToolchain.NetCoreApp22));
                Add(Job.Core.With(CsProjCoreToolchain.NetCoreApp30));
                Add(Job.Clr);
            }
        }

        byte[] data = new byte[10];
        private byte[][] keys;
        private List<byte[]> selectedKeys;

        Dictionary<LevelDbWorldKey, UInt32> dict1;
        Dictionary<LevelDbWorldKey2, UInt32> dict2;
        Dictionary<LevelDbWorldKey3, UInt32> dict3;


        [GlobalSetup]
        public void Setup()
        {
            dict1 = new Dictionary<LevelDbWorldKey, uint>();
            dict2 = new Dictionary<LevelDbWorldKey2, uint>();
            dict3 = new Dictionary<LevelDbWorldKey3, uint>();

            selectedKeys = new List<byte[]>();

            var random = new Random(132);
            keys = new byte[1000000][];
            for (int i = 0; i < keys.GetLength(0); i++)
            {
                keys[i] = new byte[10];
                random.NextBytes(keys[i]);

                var r = (uint) random.Next();
                
                dict1.Add(new LevelDbWorldKey(keys[i]), r);
                dict2.Add(new LevelDbWorldKey2(keys[i]), r);
                dict3.Add(new LevelDbWorldKey3(keys[i]), r);
            }

            for (int i = 0; i < 500000;i++)
            {
                var key = keys[random.Next(0, 500000)];
                selectedKeys.Add(key);
            }

            checkSumSum1 = 0;
            checkSumSum2 = 0;
            checkSumSum3 = 0;
        }

        ulong checkSumSum1 = 0;
        ulong checkSumSum2 = 0;
        ulong checkSumSum3 = 0;

        [GlobalCleanup]
        public void Teardown()
        {
            Console.WriteLine($"First Key {string.Join(" ", selectedKeys.First())}, last Key {string.Join(" ",selectedKeys.Last())}");
            Console.WriteLine("1: "+ checkSumSum1);
            Console.WriteLine("2: " + checkSumSum2);
            Console.WriteLine("3: " + checkSumSum3);
        }

        [Benchmark]
        public void GetChecksums1()
        {
            if (selectedKeys.Count == 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            foreach (var key in selectedKeys)
            {
                var dbKey = new LevelDbWorldKey(key);

                if (dict1.TryGetValue(dbKey, out uint checkSum))
                {
                    checkSumSum1 += checkSum;
                }
                
            }
        }

        [Benchmark]
        public void GetChecksums2()
        {
            foreach (var key in selectedKeys)
            {
                var dbKey = new LevelDbWorldKey2(key);

                if (dict2.TryGetValue(dbKey, out uint checkSum))
                {
                    checkSumSum2 += checkSum;
                }

            }
        }

        [Benchmark]
        public void GetChecksums3()
        {
            foreach (var key in selectedKeys)
            {
                var dbKey = new LevelDbWorldKey3(key);

                if (dict3.TryGetValue(dbKey, out uint checkSum))
                {
                    checkSumSum3 += checkSum;
                }

            }
        }
        //[Benchmark]
        public void InstaciateTest1()
        {
            LevelDbWorldKey k = new LevelDbWorldKey(data);
        }


        //[Benchmark]
        public void InstaciateTest2()
        {
            LevelDbWorldKey2 k = new LevelDbWorldKey2(data);
        }

        //[Benchmark]
        public void InstaciateTest3()
        {
            LevelDbWorldKey3 k = new LevelDbWorldKey3(data);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
