using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace CritBitTree.Benchmarks
{
    //[NativeMemoryProfiler]
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            BenchmarkRunner.Run<Program>();
#endif
#if DEBUG
            var program = new Program();
            program.Setup();
            program.CritBitTree();
            program.HashSet();
#endif
        }

        private const int Elements = 100;
        private readonly byte[][] _bytes = new byte[Elements][];
        private const int Lookups = 100;
        private readonly HashSet<byte[]> _hashSet = new HashSet<byte[]>();
        private readonly CritBitTree _critBitTree = new CritBitTree();

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(1234);

            for (int i = 0; i < _bytes.Length; i++)
            {
                var bytes = new byte[20];
                random.NextBytes(bytes);
                _bytes[i] = bytes;

                _hashSet.Add(bytes);
                _critBitTree.Add(bytes);
            }
        }

        [Benchmark, BenchmarkCategory("Combined")]
        public void CritBitTreeCombined()
        {
            var random = new Random(1234);

            using var critBitTree = new CritBitTree();
            foreach (var b in _bytes)
            {
                critBitTree.Add(b);
            }

            for (int i = 0; i < Lookups; i++)
            {
                critBitTree.Contains(_bytes[random.Next(0, Elements - 1)]);
            }
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Combined")]
        public void HashSetCombined()
        {
            var random = new Random(1234);

            var hashSet = new HashSet<byte[]>(new Bytearraycomparer());
            foreach (var b in _bytes)
            {
                hashSet.Add(b);
            }

            for (int i = 0; i < Lookups; i++)
            {
                hashSet.Contains(_bytes[random.Next(0, Elements - 1)]);
            }
        }

        [Benchmark, BenchmarkCategory("Add")]
        public void CritBitAdd()
        {
            using var critbit = new CritBitTree();
            foreach (var b in _bytes)
            {
                critbit.Add(b);
            }
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Add")]
        public void HashSetAdd()
        {
            var hashSet = new HashSet<byte[]>(new Bytearraycomparer());
            foreach (var b in _bytes)
            {
                hashSet.Add(b);
            }
        }

        [Benchmark, BenchmarkCategory("Contains")]
        public void CritBitContains()
        {
            var random = new Random(1234);

            for (int i = 0; i < Lookups; i++)
            {
                _critBitTree.Contains(_bytes[random.Next(0, Elements - 1)]);
            }
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Contains")]
        public void HashSetContains()
        {
            var random = new Random(1234);

            for (int i = 0; i < Lookups; i++)
            {
                _hashSet.Contains(_bytes[random.Next(0, Elements - 1)]);
            }
        }
    }
}
