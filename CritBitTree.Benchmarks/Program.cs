using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Running;

namespace CritBitTree.Benchmarks
{
    [NativeMemoryProfiler]
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class Program
    {
        static void Main(string[] args)
        {
//#if RELEASE
            BenchmarkRunner.Run<Program>();
//#endif
//#if DEBUG
//            var program = new Program();
//            program.Setup();
//            //program.CritBitTreeSafe();
//            //program.HashSet();
//#endif
        }

        private const int Elements = 1000;
        private readonly byte[][] _bytes = new byte[Elements][];
        private const int Lookups = 100;
        private readonly HashSet<byte[]> _hashSet = new HashSet<byte[]>(new Bytearraycomparer());
        private readonly CritBitTree<object> _critBitTree = new CritBitTree<object>();
        private readonly UnmanagedCritBitTree _critBitTreeUnmanaged = new UnmanagedCritBitTree();
        private readonly Dictionary<byte[], object> _dictionary = new Dictionary<byte[], object>(new Bytearraycomparer());
        private readonly ConcurrentDictionary<byte[], object> _concurrentDictionary = new ConcurrentDictionary<byte[], object>(new Bytearraycomparer());

        private static readonly char[] Chars = new char[] {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(1234);

            for (int i = 0; i < _bytes.Length; i++)
            {
                var bytes = new byte[20];
                for (int j = 0; j < 20; j++)
                {
                    bytes[j] = (byte)Chars[random.Next(0, 15)];
                }
                _bytes[i] = bytes;

                _hashSet.Add(bytes);
                _dictionary.Add(bytes, null);
                _concurrentDictionary.TryAdd(bytes, null);
                _critBitTree.Add(bytes, null);
                _critBitTreeUnmanaged.Add(bytes);
            }
        }

        //[Benchmark, BenchmarkCategory("Combined")]
        //public void CritBitTreeSafeCombined()
        //{
        //    var random = new Random(1234);

        //    using var CritBitTreeSafe = new CritBitTreeSafe();
        //    foreach (var b in _bytes)
        //    {
        //        CritBitTreeSafe.Add(b);
        //    }

        //    for (int i = 0; i < Lookups; i++)
        //    {
        //        CritBitTreeSafe.Contains(_bytes[random.Next(0, Elements - 1)]);
        //    }
        //}

        //[Benchmark(Baseline = true), BenchmarkCategory("Combined")]
        //public void HashSetCombined()
        //{
        //    var random = new Random(1234);

        //    var hashSet = new HashSet<byte[]>(new Bytearraycomparer());
        //    foreach (var b in _bytes)
        //    {
        //        hashSet.Add(b);
        //    }

        //    for (int i = 0; i < Lookups; i++)
        //    {
        //        hashSet.Contains(_bytes[random.Next(0, Elements - 1)]);
        //    }
        //}

        [Benchmark, BenchmarkCategory("Add")]
        public void CritBitAddManaged()
        {
            var critbit = new CritBitTree<object>();
            foreach (var b in _bytes)
            {
                critbit.Add(b, null);
            }
        }

        [Benchmark, BenchmarkCategory("Add")]
        public void CritBitAddUnmanaged()
        {
            using var critbit = new UnmanagedCritBitTree();
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

        //[Benchmark, BenchmarkCategory("Add")]
        //public void DictionaryAdd()
        //{
        //    var hashSet = new Dictionary<byte[], object>(new Bytearraycomparer());
        //    foreach (var b in _bytes)
        //    {
        //        hashSet.Add(b, null);
        //    }
        //}

        //[Benchmark, BenchmarkCategory("Add")]
        //public void ConcurrentDictionaryAdd()
        //{
        //    var hashSet = new ConcurrentDictionary<byte[], object>(new Bytearraycomparer());
        //    foreach (var b in _bytes)
        //    {
        //        hashSet.TryAdd(b, null);
        //    }
        //}

        [Benchmark, BenchmarkCategory("Contains")]
        public void CritBitContains()
        {
            var random = new Random(1234);

            for (int i = 0; i < Lookups; i++)
            {
                _critBitTree.ContainsKey(_bytes[random.Next(0, Elements - 1)]);
            }
        }

        [Benchmark, BenchmarkCategory("Contains")]
        public void CritBitUnmanagedContains()
        {
            var random = new Random(1234);

            for (int i = 0; i < Lookups; i++)
            {
                _critBitTreeUnmanaged.Contains(_bytes[random.Next(0, Elements - 1)]);
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

        //[Benchmark, BenchmarkCategory("Contains")]
        //public void DictionaryContains()
        //{
        //    var random = new Random(1234);

        //    for (int i = 0; i < Lookups; i++)
        //    {
        //        _dictionary.ContainsKey(_bytes[random.Next(0, Elements - 1)]);
        //    }
        //}

        //[Benchmark, BenchmarkCategory("Contains")]
        //public void ConcurrentDictionaryContains()
        //{
        //    var random = new Random(1234);

        //    for (int i = 0; i < Lookups; i++)
        //    {
        //        _concurrentDictionary.ContainsKey(_bytes[random.Next(0, Elements - 1)]);
        //    }
        //}
    }
}
