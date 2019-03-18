// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

extern alias PortableConcurrentDictionary;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using IntegerConcurrentDictionary = PortableConcurrentDictionary::System.Collections.Concurrent.ConcurrentDictionary<int, int>;

namespace Portable.ConcurrentDictionary.Tests
{
    public class ConcurrentDictionaryTests
    {
        [Fact]
        public void TestBasicScenarios()
        {
            var cd = new IntegerConcurrentDictionary();

            var tks = new Task[2];
            tks[0] = Task.Run(() =>
            {
                var ret = cd.TryAdd(1, 11);
                if (!ret)
                {
                    ret = cd.TryUpdate(1, 11, 111);
                    Assert.True(ret);
                }

                ret = cd.TryAdd(2, 22);
                if (!ret)
                {
                    ret = cd.TryUpdate(2, 22, 222);
                    Assert.True(ret);
                }
            });

            tks[1] = Task.Run(() =>
            {
                var ret = cd.TryAdd(2, 222);
                if (!ret)
                {
                    ret = cd.TryUpdate(2, 222, 22);
                    Assert.True(ret);
                }

                ret = cd.TryAdd(1, 111);
                if (!ret)
                {
                    ret = cd.TryUpdate(1, 111, 11);
                    Assert.True(ret);
                }
            });

            Task.WaitAll(tks);
        }

        [Theory]
        [InlineData(1, 1, 1, 10000)]
        [InlineData(5, 1, 1, 10000)]
        [InlineData(1, 1, 2, 5000)]
        [InlineData(1, 1, 5, 2000)]
        [InlineData(4, 0, 4, 2000)]
        [InlineData(16, 31, 4, 2000)]
        [InlineData(64, 5, 5, 5000)]
        [InlineData(5, 5, 5, 2500)]
        public void TestAdd1(int cLevel, int initSize, int threads, int addsPerThread)
        {
            var dictConcurrent = new IntegerConcurrentDictionary(cLevel, 1);
            IDictionary<int, int> dict = dictConcurrent;

            var count = threads;
            using (var mre = new ManualResetEvent(false))
            {
                for (var i = 0; i < threads; i++)
                {
                    var ii = i;
                    Task.Run(
                        () =>
                        {
                            for (var j = 0; j < addsPerThread; j++)
                            {
                                dict.Add(j + ii * addsPerThread, -(j + ii * addsPerThread));
                            }
                            if (Interlocked.Decrement(ref count) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }

            foreach (var pair in dict)
            {
                Assert.Equal(pair.Key, -pair.Value);
            }

            var gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);

            gotKeys.Sort();

            var expectKeys = new List<int>();
            var itemCount = threads * addsPerThread;
            for (var i = 0; i < itemCount; i++)
                expectKeys.Add(i);

            Assert.Equal(expectKeys.Count, gotKeys.Count);

            for (var i = 0; i < expectKeys.Count; i++)
            {
                Assert.True(expectKeys[i].Equals(gotKeys[i]),
                    string.Format("The set of keys in the dictionary is are not the same as the expected" + Environment.NewLine +
                            "TestAdd1(cLevel={0}, initSize={1}, threads={2}, addsPerThread={3})", cLevel, initSize, threads, addsPerThread)
                   );
            }

            // Finally, let's verify that the count is reported correctly.
            var expectedCount = threads * addsPerThread;
            Assert.Equal(expectedCount, dict.Count);
            Assert.Equal(expectedCount, dictConcurrent.ToArray().Length);
        }

        [Theory]
        [InlineData(1, 1, 10000)]
        [InlineData(5, 1, 10000)]
        [InlineData(1, 2, 5000)]
        [InlineData(1, 5, 2001)]
        [InlineData(4, 4, 2001)]
        [InlineData(15, 5, 2001)]
        [InlineData(64, 5, 5000)]
        [InlineData(5, 5, 25000)]
        public void TestUpdate1(int cLevel, int threads, int updatesPerThread)
        {
            IDictionary<int, int> dict = new IntegerConcurrentDictionary(cLevel, 1);

            for (var i = 1; i <= updatesPerThread; i++) dict[i] = i;

            var running = threads;
            using (var mre = new ManualResetEvent(false))
            {
                for (var i = 0; i < threads; i++)
                {
                    var ii = i;
                    Task.Run(
                        () =>
                        {
                            for (var j = 1; j <= updatesPerThread; j++)
                            {
                                dict[j] = (ii + 2) * j;
                            }
                            if (Interlocked.Decrement(ref running) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }

            foreach (var pair in dict)
            {
                var div = pair.Value / pair.Key;
                var rem = pair.Value % pair.Key;

                Assert.Equal(0, rem);
                Assert.True(div > 1 && div <= threads + 1,
                    string.Format("* Invalid value={3}! TestUpdate1(cLevel={0}, threads={1}, updatesPerThread={2})", cLevel, threads, updatesPerThread, div));
            }

            var gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();

            var expectKeys = new List<int>();
            for (var i = 1; i <= updatesPerThread; i++)
                expectKeys.Add(i);

            Assert.Equal(expectKeys.Count, gotKeys.Count);

            for (var i = 0; i < expectKeys.Count; i++)
            {
                Assert.True(expectKeys[i].Equals(gotKeys[i]),
                   string.Format("The set of keys in the dictionary is are not the same as the expected." + Environment.NewLine +
                           "TestUpdate1(cLevel={0}, threads={1}, updatesPerThread={2})", cLevel, threads, updatesPerThread)
                  );
            }
        }

        [Theory]
        [InlineData(1, 1, 10000)]
        [InlineData(5, 1, 10000)]
        [InlineData(1, 2, 5000)]
        [InlineData(1, 5, 2001)]
        [InlineData(4, 4, 2001)]
        [InlineData(15, 5, 2001)]
        [InlineData(64, 5, 5000)]
        [InlineData(5, 5, 25000)]
        public void TestRead1(int cLevel, int threads, int readsPerThread)
        {
            IDictionary<int, int> dict = new IntegerConcurrentDictionary(cLevel, 1);

            for (var i = 0; i < readsPerThread; i += 2) dict[i] = i;

            var count = threads;
            using (var mre = new ManualResetEvent(false))
            {
                for (var i = 0; i < threads; i++)
                {
                    var ii = i;
                    Task.Run(
                        () =>
                        {
                            for (var j = 0; j < readsPerThread; j++)
                            {
                                var val = 0;
                                if (dict.TryGetValue(j, out val))
                                {
                                    Assert.Equal(0, j % 2);
                                    Assert.Equal(j, val);
                                }
                                else
                                {
                                    Assert.Equal(1, j % 2);
                                }
                            }
                            if (Interlocked.Decrement(ref count) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }
        }

        [Theory]
        [InlineData(1, 1, 10000)]
        [InlineData(5, 1, 10000)]
        [InlineData(1, 2, 5000)]
        [InlineData(1, 5, 2001)]
        [InlineData(4, 4, 2001)]
        [InlineData(15, 5, 2001)]
        [InlineData(64, 5, 5000)]
        public void TestRemove1(int cLevel, int threads, int removesPerThread)
        {
            var dict = new IntegerConcurrentDictionary(cLevel, 1);
            var methodparameters = string.Format("* TestRemove1(cLevel={0}, threads={1}, removesPerThread={2})", cLevel, threads, removesPerThread);
            var N = 2 * threads * removesPerThread;

            for (var i = 0; i < N; i++) dict[i] = -i;

            // The dictionary contains keys [0..N), each key mapped to a value equal to the key.
            // Threads will cooperatively remove all even keys

            var running = threads;
            using (var mre = new ManualResetEvent(false))
            {
                for (var i = 0; i < threads; i++)
                {
                    var ii = i;
                    Task.Run(
                        () =>
                        {
                            for (var j = 0; j < removesPerThread; j++)
                            {
                                int value;
                                var key = 2 * (ii + j * threads);
                                Assert.True(dict.TryRemove(key, out value), "Failed to remove an element! " + methodparameters);

                                Assert.Equal(-key, value);
                            }

                            if (Interlocked.Decrement(ref running) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }

            foreach (var pair in dict)
            {
                Assert.Equal(pair.Key, -pair.Value);
            }

            var gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();

            var expectKeys = new List<int>();
            for (var i = 0; i < (threads * removesPerThread); i++)
                expectKeys.Add(2 * i + 1);

            Assert.Equal(expectKeys.Count, gotKeys.Count);

            for (var i = 0; i < expectKeys.Count; i++)
            {
                Assert.True(expectKeys[i].Equals(gotKeys[i]), "  > Unexpected key value! " + methodparameters);
            }

            // Finally, let's verify that the count is reported correctly.
            Assert.Equal(expectKeys.Count, dict.Count);
            Assert.Equal(expectKeys.Count, dict.ToArray().Length);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(5000)]
        public void TestRemove2(int removesPerThread)
        {
            var dict = new IntegerConcurrentDictionary();

            for (var i = 0; i < removesPerThread; i++) dict[i] = -i;

            // The dictionary contains keys [0..N), each key mapped to a value equal to the key.
            // Threads will cooperatively remove all even keys.
            const int SIZE = 2;
            var running = SIZE;

            var seen = new bool[SIZE][];
            for (var i = 0; i < SIZE; i++) seen[i] = new bool[removesPerThread];

            using (var mre = new ManualResetEvent(false))
            {
                for (var t = 0; t < SIZE; t++)
                {
                    var thread = t;
                    Task.Run(
                        () =>
                        {
                            for (var key = 0; key < removesPerThread; key++)
                            {
                                int value;
                                if (dict.TryRemove(key, out value))
                                {
                                    seen[thread][key] = true;

                                    Assert.Equal(-key, value);
                                }
                            }
                            if (Interlocked.Decrement(ref running) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }

            Assert.Empty(dict);

            for (var i = 0; i < removesPerThread; i++)
            {
                Assert.False(seen[0][i] == seen[1][i],
                    string.Format("> FAILED. Two threads appear to have removed the same element. TestRemove2(removesPerThread={0})", removesPerThread)
                    );
            }
        }

        [Fact]
        public static void TestRemove3()
        {
            var dict = new IntegerConcurrentDictionary();

            dict[99] = -99;

            ICollection<KeyValuePair<int, int>> col = dict;

            // Make sure we cannot "remove" a key/value pair which is not in the dictionary
            for (var i = 0; i < 200; i++)
            {
                if (i != 99)
                {
                    Assert.False(col.Remove(new KeyValuePair<int, int>(i, -99)), "Should not remove not existing a key/value pair - new KeyValuePair<int, int>(i, -99)");
                    Assert.False(col.Remove(new KeyValuePair<int, int>(99, -i)), "Should not remove not existing a key/value pair - new KeyValuePair<int, int>(99, -i)");
                }
            }

            // Can we remove a key/value pair successfully?
            Assert.True(col.Remove(new KeyValuePair<int, int>(99, -99)), "Failed to remove existing key/value pair");

            // Make sure the key/value pair is gone
            Assert.False(col.Remove(new KeyValuePair<int, int>(99, -99)), "Should not remove the key/value pair which has been removed");

            // And that the dictionary is empty. We will check the count in a few different ways:
            Assert.Empty(dict);
            Assert.Empty(dict.ToArray());
        }

        [Fact]
        public static void TestGetOrAdd()
        {
            TestGetOrAddOrUpdate(1, 1, 1, 10000, true);
            TestGetOrAddOrUpdate(5, 1, 1, 10000, true);
            TestGetOrAddOrUpdate(1, 1, 2, 5000, true);
            TestGetOrAddOrUpdate(1, 1, 5, 2000, true);
            TestGetOrAddOrUpdate(4, 0, 4, 2000, true);
            TestGetOrAddOrUpdate(16, 31, 4, 2000, true);
            TestGetOrAddOrUpdate(64, 5, 5, 5000, true);
            TestGetOrAddOrUpdate(5, 5, 5, 25000, true);
        }

        [Fact]
        public static void TestAddOrUpdate()
        {
            TestGetOrAddOrUpdate(1, 1, 1, 10000, false);
            TestGetOrAddOrUpdate(5, 1, 1, 10000, false);
            TestGetOrAddOrUpdate(1, 1, 2, 5000, false);
            TestGetOrAddOrUpdate(1, 1, 5, 2000, false);
            TestGetOrAddOrUpdate(4, 0, 4, 2000, false);
            TestGetOrAddOrUpdate(16, 31, 4, 2000, false);
            TestGetOrAddOrUpdate(64, 5, 5, 5000, false);
            TestGetOrAddOrUpdate(5, 5, 5, 25000, false);
        }

        private static void TestGetOrAddOrUpdate(int cLevel, int initSize, int threads, int addsPerThread, bool isAdd)
        {
            var dict = new IntegerConcurrentDictionary(cLevel, 1);

            var count = threads;
            using (var mre = new ManualResetEvent(false))
            {
                for (var i = 0; i < threads; i++)
                {
                    var ii = i;
                    Task.Run(
                        () =>
                        {
                            for (var j = 0; j < addsPerThread; j++)
                            {
                                if (isAdd)
                                {
                                    //call either of the two overloads of GetOrAdd
                                    if (j + ii % 2 == 0)
                                    {
                                        dict.GetOrAdd(j, -j);
                                    }
                                    else
                                    {
                                        dict.GetOrAdd(j, x => -x);
                                    }
                                }
                                else
                                {
                                    if (j + ii % 2 == 0)
                                    {
                                        dict.AddOrUpdate(j, -j, (k, v) => -j);
                                    }
                                    else
                                    {
                                        dict.AddOrUpdate(j, (k) => -k, (k, v) => -k);
                                    }
                                }
                            }
                            if (Interlocked.Decrement(ref count) == 0) mre.Set();
                        });
                }
                mre.WaitOne();
            }

            foreach (var pair in dict)
            {
                Assert.Equal(pair.Key, -pair.Value);
            }

            var gotKeys = new List<int>();
            foreach (var pair in dict)
                gotKeys.Add(pair.Key);
            gotKeys.Sort();

            var expectKeys = new List<int>();
            for (var i = 0; i < addsPerThread; i++)
                expectKeys.Add(i);

            Assert.Equal(expectKeys.Count, gotKeys.Count);

            for (var i = 0; i < expectKeys.Count; i++)
            {
                Assert.True(expectKeys[i].Equals(gotKeys[i]),
                    string.Format("* Test '{4}': Level={0}, initSize={1}, threads={2}, addsPerThread={3})" + Environment.NewLine +
                    "> FAILED.  The set of keys in the dictionary is are not the same as the expected.",
                    cLevel, initSize, threads, addsPerThread, isAdd ? "GetOrAdd" : "GetOrUpdate"));
            }

            // Finally, let's verify that the count is reported correctly.
            Assert.Equal(addsPerThread, dict.Count);
            Assert.Equal(addsPerThread, dict.ToArray().Length);
        }

        [Fact]
        public static void TestBugFix669376()
        {
            var cd = new ConcurrentDictionary<string, int>(new OrdinalStringComparer());
            cd["test"] = 10;
            Assert.True(cd.ContainsKey("TEST"), "Customized comparer didn't work");
        }

        private class OrdinalStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                var xlower = x.ToLowerInvariant();
                var ylower = y.ToLowerInvariant();
                return string.CompareOrdinal(xlower, ylower) == 0;
            }

            public int GetHashCode(string obj)
            {
                return 0;
            }
        }

        [Fact]
        public static void TestConstructor()
        {
            var dictionary = new IntegerConcurrentDictionary(new[] { new KeyValuePair<int, int>(1, 1) });
            Assert.False(dictionary.IsEmpty);
            Assert.Equal(1, dictionary.Keys.Count);
            Assert.Equal(1, dictionary.Values.Count);
        }

        [Fact]
        public static void TestDebuggerAttributes()
        {
            //TODO
            //DebuggerAttributes.ValidateDebuggerDisplayReferences(new ConcurrentDictionary<string, int>());
            //DebuggerAttributes.ValidateDebuggerTypeProxyProperties(new ConcurrentDictionary<string, int>());
        }

        [Fact]
        public static void TestConstructor_Negative()
        {
            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary((ICollection<KeyValuePair<int, int>>)null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null collection is passed");

            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary((IEqualityComparer<int>)null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null IEqualityComparer is passed");

            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary((ICollection<KeyValuePair<int, int>>)null, EqualityComparer<int>.Default));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null collection and non null IEqualityComparer passed");

            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary(new[] { new KeyValuePair<int, int>(1, 1) }, null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when non null collection and null IEqualityComparer passed");

            Assert.Throws<ArgumentNullException>(
               () => new ConcurrentDictionary<string, int>(new[] { new KeyValuePair<string, int>(null, 1) }));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when collection has null key passed");
            Assert.Throws<ArgumentException>(
               () => new IntegerConcurrentDictionary(new[] { new KeyValuePair<int, int>(1, 1), new KeyValuePair<int, int>(1, 2) }));
            // "TestConstructor:  FAILED.  Constructor didn't throw AE when collection has duplicate keys passed");

            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary(1, null, EqualityComparer<int>.Default));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null collection is passed");

            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary(1, new[] { new KeyValuePair<int, int>(1, 1) }, null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null comparer is passed");

            Assert.Throws<ArgumentNullException>(
               () => new IntegerConcurrentDictionary(1, 1, null));
            // "TestConstructor:  FAILED.  Constructor didn't throw ANE when null comparer is passed");

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new IntegerConcurrentDictionary(0, 10));
            // "TestConstructor:  FAILED.  Constructor didn't throw AORE when <1 concurrencyLevel passed");

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new IntegerConcurrentDictionary(-1, 0));
            // "TestConstructor:  FAILED.  Constructor didn't throw AORE when < 0 capacity passed");
        }

        [Fact]
        public static void TestExceptions()
        {
            var dictionary = new ConcurrentDictionary<string, int>();
            Assert.Throws<ArgumentNullException>(
               () => dictionary.TryAdd(null, 0));
            //  "TestExceptions:  FAILED.  TryAdd didn't throw ANE when null key is passed");

            Assert.Throws<ArgumentNullException>(
               () => dictionary.ContainsKey(null));
            // "TestExceptions:  FAILED.  Contains didn't throw ANE when null key is passed");

            int item;
            Assert.Throws<ArgumentNullException>(
               () => dictionary.TryRemove(null, out item));
            //  "TestExceptions:  FAILED.  TryRemove didn't throw ANE when null key is passed");
            Assert.Throws<ArgumentNullException>(
               () => dictionary.TryGetValue(null, out item));
            // "TestExceptions:  FAILED.  TryGetValue didn't throw ANE when null key is passed");

            Assert.Throws<ArgumentNullException>(
               () => { var x = dictionary[null]; });
            // "TestExceptions:  FAILED.  this[] didn't throw ANE when null key is passed");
            Assert.Throws<KeyNotFoundException>(
               () => { var x = dictionary["1"]; });
            // "TestExceptions:  FAILED.  this[] TryGetValue didn't throw KeyNotFoundException!");

            Assert.Throws<ArgumentNullException>(
               () => dictionary[null] = 1);
            // "TestExceptions:  FAILED.  this[] didn't throw ANE when null key is passed");

            Assert.Throws<ArgumentNullException>(
               () => dictionary.GetOrAdd(null, (k) => 0));
            // "TestExceptions:  FAILED.  GetOrAdd didn't throw ANE when null key is passed");
            Assert.Throws<ArgumentNullException>(
               () => dictionary.GetOrAdd("1", null));
            // "TestExceptions:  FAILED.  GetOrAdd didn't throw ANE when null valueFactory is passed");
            Assert.Throws<ArgumentNullException>(
               () => dictionary.GetOrAdd(null, 0));
            // "TestExceptions:  FAILED.  GetOrAdd didn't throw ANE when null key is passed");

            Assert.Throws<ArgumentNullException>(
               () => dictionary.AddOrUpdate(null, (k) => 0, (k, v) => 0));
            // "TestExceptions:  FAILED.  AddOrUpdate didn't throw ANE when null key is passed");
            Assert.Throws<ArgumentNullException>(
               () => dictionary.AddOrUpdate("1", null, (k, v) => 0));
            // "TestExceptions:  FAILED.  AddOrUpdate didn't throw ANE when null updateFactory is passed");
            Assert.Throws<ArgumentNullException>(
               () => dictionary.AddOrUpdate(null, (k) => 0, null));
            // "TestExceptions:  FAILED.  AddOrUpdate didn't throw ANE when null addFactory is passed");

            dictionary.TryAdd("1", 1);
            Assert.Throws<ArgumentException>(
               () => ((IDictionary<string, int>)dictionary).Add("1", 2));
            // "TestExceptions:  FAILED.  IDictionary didn't throw AE when duplicate key is passed");
        }

        [Fact]
        public static void TestIDictionary()
        {
            IDictionary dictionary = new ConcurrentDictionary<string, int>();
            Assert.False(dictionary.IsReadOnly);

            // Empty dictionary should not enumerate
            Assert.Empty(dictionary);

            const int SIZE = 10;
            for (var i = 0; i < SIZE; i++)
                dictionary.Add(i.ToString(), i);

            Assert.Equal(SIZE, dictionary.Count);

            //test contains
            Assert.False(dictionary.Contains(1), "TestIDictionary:  FAILED.  Contain returned true for incorrect key type");
            Assert.False(dictionary.Contains("100"), "TestIDictionary:  FAILED.  Contain returned true for incorrect key");
            Assert.True(dictionary.Contains("1"), "TestIDictionary:  FAILED.  Contain returned false for correct key");

            //test GetEnumerator
            var count = 0;
            foreach (var obj in dictionary)
            {
                var entry = (DictionaryEntry)obj;
                var key = (string)entry.Key;
                var value = (int)entry.Value;
                var expectedValue = int.Parse(key);
                Assert.True(value == expectedValue,
                    string.Format("TestIDictionary:  FAILED.  Unexpected value returned from GetEnumerator, expected {0}, actual {1}", value, expectedValue));
                count++;
            }

            Assert.Equal(SIZE, count);
            Assert.Equal(SIZE, dictionary.Keys.Count);
            Assert.Equal(SIZE, dictionary.Values.Count);

            //Test Remove
            dictionary.Remove("9");
            Assert.Equal(SIZE - 1, dictionary.Count);

            //Test this[]
            for (var i = 0; i < dictionary.Count; i++)
                Assert.Equal(i, (int)dictionary[i.ToString()]);

            dictionary["1"] = 100; // try a valid setter
            Assert.Equal(100, (int)dictionary["1"]);

            //nonsexist key
            Assert.Null(dictionary["NotAKey"]);
        }

        [Fact]
        public static void TestIDictionary_Negative()
        {
            IDictionary dictionary = new ConcurrentDictionary<string, int>();
            Assert.Throws<ArgumentNullException>(
               () => dictionary.Add(null, 1));
            // "TestIDictionary:  FAILED.  Add didn't throw ANE when null key is passed");

            Assert.Throws<ArgumentException>(
               () => dictionary.Add(1, 1));
            // "TestIDictionary:  FAILED.  Add didn't throw AE when incorrect key type is passed");

            Assert.Throws<ArgumentException>(
               () => dictionary.Add("1", "1"));
            // "TestIDictionary:  FAILED.  Add didn't throw AE when incorrect value type is passed");

            Assert.Throws<ArgumentNullException>(
               () => dictionary.Contains(null));
            // "TestIDictionary:  FAILED.  Contain didn't throw ANE when null key is passed");

            //Test Remove
            Assert.Throws<ArgumentNullException>(
               () => dictionary.Remove(null));
            // "TestIDictionary:  FAILED.  Remove didn't throw ANE when null key is passed");

            //Test this[]
            Assert.Throws<ArgumentNullException>(
               () => { var val = dictionary[null]; });
            // "TestIDictionary:  FAILED.  this[] getter didn't throw ANE when null key is passed");
            Assert.Throws<ArgumentNullException>(
               () => dictionary[null] = 0);
            // "TestIDictionary:  FAILED.  this[] setter didn't throw ANE when null key is passed");

            Assert.Throws<ArgumentException>(
               () => dictionary[1] = 0);
            // "TestIDictionary:  FAILED.  this[] setter didn't throw AE when invalid key type is passed");

            Assert.Throws<ArgumentException>(
               () => dictionary["1"] = "0");
            // "TestIDictionary:  FAILED.  this[] setter didn't throw AE when invalid value type is passed");
        }

        [Fact]
        public static void TestICollection()
        {
            ICollection dictionary = new IntegerConcurrentDictionary();
            Assert.False(dictionary.IsSynchronized, "TestICollection:  FAILED.  IsSynchronized returned true!");

            var key = -1;
            var value = +1;
            //add one item to the dictionary
            ((IntegerConcurrentDictionary)dictionary).TryAdd(key, value);

            var objectArray = new object[1];
            dictionary.CopyTo(objectArray, 0);

            Assert.Equal(key, ((KeyValuePair<int, int>)objectArray[0]).Key);
            Assert.Equal(value, ((KeyValuePair<int, int>)objectArray[0]).Value);

            var keyValueArray = new KeyValuePair<int, int>[1];
            dictionary.CopyTo(keyValueArray, 0);
            Assert.Equal(key, keyValueArray[0].Key);
            Assert.Equal(value, keyValueArray[0].Value);

            var entryArray = new DictionaryEntry[1];
            dictionary.CopyTo(entryArray, 0);
            Assert.Equal(key, (int)entryArray[0].Key);
            Assert.Equal(value, (int)entryArray[0].Value);
        }

        [Fact]
        public static void TestICollection_Negative()
        {
            ICollection dictionary = new IntegerConcurrentDictionary();
            Assert.False(dictionary.IsSynchronized, "TestICollection:  FAILED.  IsSynchronized returned true!");

            Assert.Throws<NotSupportedException>(() => { var obj = dictionary.SyncRoot; });
            // "TestICollection:  FAILED.  SyncRoot property didn't throw");
            Assert.Throws<ArgumentNullException>(() => dictionary.CopyTo(null, 0));
            // "TestICollection:  FAILED.  CopyTo didn't throw ANE when null Array is passed");
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.CopyTo(new object[] { }, -1));
            // "TestICollection:  FAILED.  CopyTo didn't throw AORE when negative index passed");

            //add one item to the dictionary
            ((IntegerConcurrentDictionary)dictionary).TryAdd(1, 1);
            Assert.Throws<ArgumentException>(() => dictionary.CopyTo(new object[] { }, 0));
            // "TestICollection:  FAILED.  CopyTo didn't throw AE when the Array size is smaller than the dictionary count");
        }

        [Fact]
        public static void TestClear()
        {
            var dictionary = new IntegerConcurrentDictionary();
            for (var i = 0; i < 10; i++)
                dictionary.TryAdd(i, i);

            Assert.Equal(10, dictionary.Count);

            dictionary.Clear();
            Assert.Empty(dictionary);

            int item;
            Assert.False(dictionary.TryRemove(1, out item), "TestClear: FAILED.  TryRemove succeeded after Clear");
            Assert.True(dictionary.IsEmpty, "TestClear: FAILED.  IsEmpty returned false after Clear");
        }

        [Fact]
        public static void TestTryUpdate()
        {
            var dictionary = new ConcurrentDictionary<string, int>();
            Assert.Throws<ArgumentNullException>(
               () => dictionary.TryUpdate(null, 0, 0));
            // "TestTryUpdate:  FAILED.  TryUpdate didn't throw ANE when null key is passed");

            for (var i = 0; i < 10; i++)
                dictionary.TryAdd(i.ToString(), i);

            for (var i = 0; i < 10; i++)
            {
                Assert.True(dictionary.TryUpdate(i.ToString(), i + 1, i), "TestTryUpdate:  FAILED.  TryUpdate failed!");
                Assert.Equal(i + 1, dictionary[i.ToString()]);
            }

            //test TryUpdate concurrently
            dictionary.Clear();
            for (var i = 0; i < 1000; i++)
                dictionary.TryAdd(i.ToString(), i);

            var mres = new ManualResetEventSlim();
            var tasks = new Task[10];
            var updatedKeys = new ThreadLocal<ThreadData>(true);
            for (var i = 0; i < tasks.Length; i++)
            {
                // We are creating the Task using TaskCreationOptions.LongRunning because...
                // there is no guarantee that the Task will be created on another thread.
                // There is also no guarantee that using this TaskCreationOption will force
                // it to be run on another thread.
                tasks[i] = Task.Factory.StartNew((obj) =>
                {
                    mres.Wait();
                    var index = (((int)obj) + 1) + 1000;
                    updatedKeys.Value = new ThreadData();
                    updatedKeys.Value.ThreadIndex = index;

                    for (var j = 0; j < dictionary.Count; j++)
                    {
                        if (dictionary.TryUpdate(j.ToString(), index, j))
                        {
                            if (dictionary[j.ToString()] != index)
                            {
                                updatedKeys.Value.Succeeded = false;
                                return;
                            }
                            updatedKeys.Value.Keys.Add(j.ToString());
                        }
                    }
                }, i, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            mres.Set();
            Task.WaitAll(tasks);

            var numberSucceeded = 0;
            var totalKeysUpdated = 0;
            foreach (var threadData in updatedKeys.Values)
            {
                totalKeysUpdated += threadData.Keys.Count;
                if (threadData.Succeeded)
                    numberSucceeded++;
            }

            Assert.True(numberSucceeded == tasks.Length, "One or more threads failed!");
            Assert.True(totalKeysUpdated == dictionary.Count,
               string.Format("TestTryUpdate:  FAILED.  The updated keys count doesn't match the dictionary count, expected {0}, actual {1}", dictionary.Count, totalKeysUpdated));
            foreach (var value in updatedKeys.Values)
            {
                for (var i = 0; i < value.Keys.Count; i++)
                    Assert.True(dictionary[value.Keys[i]] == value.ThreadIndex,
                       string.Format("TestTryUpdate:  FAILED.  The updated value doesn't match the thread index, expected {0} actual {1}", value.ThreadIndex, dictionary[value.Keys[i]]));
            }

            //test TryUpdate with non atomic values (intPtr > 8)
            var dict = new ConcurrentDictionary<int, Struct16>();
            dict.TryAdd(1, new Struct16(1, -1));
            Assert.True(dict.TryUpdate(1, new Struct16(2, -2), new Struct16(1, -1)), "TestTryUpdate:  FAILED.  TryUpdate failed for non atomic values ( > 8 bytes)");
        }

        #region Helper Classes and Methods

        private class ThreadData
        {
            public int ThreadIndex;
            public bool Succeeded = true;
            public List<string> Keys = new List<string>();
        }

        private struct Struct16 : IEqualityComparer<Struct16>
        {
            public long L1, L2;
            public Struct16(long l1, long l2)
            {
                L1 = l1;
                L2 = l2;
            }

            public bool Equals(Struct16 x, Struct16 y)
            {
                return x.L1 == y.L1 && x.L2 == y.L2;
            }

            public int GetHashCode(Struct16 obj)
            {
                return (int)L1;
            }
        }

        #endregion
    }
}