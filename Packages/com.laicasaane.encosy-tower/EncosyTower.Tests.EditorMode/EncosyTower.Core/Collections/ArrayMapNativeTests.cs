#if UNITY_COLLECTIONS

using System;
using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.TypeWraps;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public partial class ArrayMapNativeTests
    {
        [WrapRecord]
        public readonly partial record struct UintId(uint Value);

        [Test]
        public void Constructor_CreatesValidMap()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.IsTrue(map.IsCreated);
            Assert.AreEqual(0, map.Count);
            Assert.GreaterOrEqual(map.Capacity, 4);
        }

        [Test]
        public void DefaultMap_IsNotCreated()
        {
            ArrayMapNative<int, int> map = default;

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void Add_StoresValues()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
            Assert.AreEqual(30, map[3]);
        }

        [Test]
        public void Add_DuplicateKey_Throws()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.Throws<InvalidOperationException>(() => map.Add(1, 20));
        }

        [Test]
        public void Indexer_Set_AddsOrOverwrites()
        {
            // Not using a 'using' variable because the indexer setter cannot be
            // invoked on a readonly local (CS1654).
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            try
            {
                map[1] = 10;
                Assert.AreEqual(1, map.Count);
                Assert.AreEqual(10, map[1]);

                map[1] = 99;
                Assert.AreEqual(1, map.Count);
                Assert.AreEqual(99, map[1]);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void TryAdd_ReturnsFalseOnDuplicate()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.IsTrue(map.TryAdd(1, 10, out var index0));
            Assert.AreEqual(0, index0);

            Assert.IsFalse(map.TryAdd(1, 20, out var index1));
            Assert.AreEqual(0, index1);
            Assert.AreEqual(10, map[1]);
        }

        [Test]
        public void ContainsKey_ReflectsContent()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsFalse(map.ContainsKey(2));
        }

        [Test]
        public void TryGetValue_ReturnsExpected()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.IsTrue(map.TryGetValue(1, out var value1));
            Assert.AreEqual(10, value1);

            Assert.IsTrue(map.TryGetValue(2, out var value2));
            Assert.AreEqual(20, value2);

            Assert.IsFalse(map.TryGetValue(3, out var missing));
            Assert.AreEqual(0, missing);
        }

        [Test]
        public void FindIndex_ReturnsMinusOneWhenMissing()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.AreEqual(0, map.FindIndex(1));
            Assert.AreEqual(-1, map.FindIndex(2));
        }

        [Test]
        public void GetOrAdd_AddsWhenMissing()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            ref var value = ref map.GetOrAdd(1);
            Assert.AreEqual(0, value);
            value = 42;

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(42, map[1]);
        }

        [Test]
        public void GetOrAdd_ReturnsExistingByRef()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            ref var value = ref map.GetOrAdd(1);
            Assert.AreEqual(10, value);
            value = 20;

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(20, map[1]);
        }

        [Test]
        public void GetOrAdd_WithBuilder_UsesBuilderOnlyWhenMissing()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            var value = map.GetOrAdd(1, () => 7);
            Assert.AreEqual(7, value);

            var existing = map.GetOrAdd(1, () => 999);
            Assert.AreEqual(7, existing);

            map.Dispose();
        }

        [Test]
        public void GetOrAdd_OutIndex_ReportsIndex()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            map.GetOrAdd(1, out var index0) = 5;
            Assert.AreEqual(0, index0);

            map.GetOrAdd(2, out var index1) = 6;
            Assert.AreEqual(1, index1);

            map.GetOrAdd(1, out var indexExisting);
            Assert.AreEqual(0, indexExisting);
        }

        [Test]
        public void GetOrAdd_WithFuncRef_PassesParameter()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            var param = 3;
            var value = map.GetOrAdd(1, (ref int p) => p * 10, ref param);

            Assert.AreEqual(30, value);

            map.Dispose();
        }

        [Test]
        public void GetValueByRef_ReturnsRef()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            ref var value = ref map.GetValueByRef(1);
            value = 99;

            Assert.AreEqual(99, map[1]);
        }

        [Test]
        public void GetValueByRef_MissingKey_Throws()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            Assert.Throws<KeyNotFoundException>(() => map.GetValueByRef(99));
        }

        [Test]
        public void Remove_RemovesKey()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            Assert.IsTrue(map.Remove(2));

            Assert.AreEqual(2, map.Count);
            Assert.IsFalse(map.ContainsKey(2));
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(30, map[3]);
        }

        [Test]
        public void Remove_MissingKey_ReturnsFalse()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);

            Assert.IsFalse(map.Remove(99));
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void Remove_OutValue_ReportsRemoved()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.IsTrue(map.Remove(1, out var index, out var value));
            Assert.AreEqual(10, value);
            Assert.GreaterOrEqual(index, 0);
        }

        [Test]
        public void Remove_LastInserted_KeepsRest()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 5; i++)
            {
                map.Add(i, i * 10);
            }

            Assert.IsTrue(map.Remove(4));

            Assert.AreEqual(4, map.Count);

            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(i * 10, map[i]);
            }
        }

        [Test]
        public void RemoveAndReadd_StaysConsistent()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 10; i++)
            {
                map.Add(i, i);
            }

            for (var i = 0; i < 10; i += 2)
            {
                Assert.IsTrue(map.Remove(i));
            }

            Assert.AreEqual(5, map.Count);

            for (var i = 0; i < 10; i += 2)
            {
                map.Add(i, i + 100);
            }

            Assert.AreEqual(10, map.Count);

            for (var i = 0; i < 10; i++)
            {
                var expected = (i % 2 == 0) ? i + 100 : i;
                Assert.AreEqual(expected, map[i]);
            }
        }

        [Test]
        public void Clear_ResetsCount()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Clear();

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));

            map.Add(1, 99);
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(99, map[1]);
        }

        [Test]
        public void Recycle_ResetsCountButKeepsValuesArray()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Recycle();

            Assert.AreEqual(0, map.Count);
            Assert.IsFalse(map.ContainsKey(1));
        }

        [Test]
        public void Grow_PastInitialCapacity_KeepsAllEntries()
        {
            const int N = 200;
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                map.Add(i, i * 3);
            }

            Assert.AreEqual(N, map.Count);

            for (var i = 0; i < N; i++)
            {
                Assert.AreEqual(i * 3, map[i], $"key {i}");
            }
        }

        [Test]
        public void EnsureCapacity_GrowsCapacity()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var before = map.Capacity;

            map.EnsureCapacity(before + 100);

            Assert.Greater(map.Capacity, before);
        }

        [Test]
        public void IncreaseCapacityBy_GrowsCapacity()
        {
            using var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            var before = map.Capacity;

            map.IncreaseCapacityBy(50);

            Assert.GreaterOrEqual(map.Capacity, before + 50);
        }

        [Test]
        public void Trim_ShrinksCapacityToCount()
        {
            using var map = new ArrayMapNative<int, int>(64, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            map.Trim();

            Assert.AreEqual(2, map.Capacity);
            Assert.AreEqual(10, map[1]);
            Assert.AreEqual(20, map[2]);
        }

        [Test]
        public void Enumerator_VisitsAllPairs()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 5; i++)
            {
                map.Add(i, i * 10);
            }

            var seen = new Dictionary<int, int>();

            foreach (var pair in map)
            {
                seen[pair.Key] = pair.Value;
            }

            Assert.AreEqual(5, seen.Count);

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(i * 10, seen[i]);
            }
        }

        [Test]
        public void Enumerator_ModifyDuringIteration_Throws()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var pair in map)
                {
                    map.Add(pair.Key + 100, 0);
                }
            });
        }

        [Test]
        public void Keys_EnumeratesAllKeys()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 4; i++)
            {
                map.Add(i, i);
            }

            var keys = new HashSet<int>();

            foreach (var key in map.Keys)
            {
                keys.Add(key);
            }

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, keys);
        }

        [Test]
        public void Values_SliceMatchesCount()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);
            map.Add(3, 30);

            var values = map.Values;

            Assert.AreEqual(3, values.Length);
        }

        [Test]
        public void CopyConstructor_DeepCopiesContent()
        {
            using var source = new ArrayMapNative<int, int>(8, Allocator.Temp);
            source.Add(1, 10);
            source.Add(2, 20);

            using var copy = new ArrayMapNative<int, int>(source, Allocator.Temp);

            Assert.AreEqual(source.Count, copy.Count);
            Assert.AreEqual(10, copy[1]);
            Assert.AreEqual(20, copy[2]);

            copy.Add(3, 30);
            Assert.IsFalse(source.ContainsKey(3));
        }

        [Test]
        public void CopyConstructor_FromInvalidSource_Throws()
        {
            ArrayMapNative<int, int> source = default;

            Assert.Throws<InvalidOperationException>(() =>
            {
                using var copy = new ArrayMapNative<int, int>(source, Allocator.Temp);
            });
        }

        [Test]
        public void Reinterpret_ReusesUnderlyingValues()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 0);

            var reinterpreted = map.Reinterpret<uint>();

            Assert.AreEqual(map.Count, reinterpreted.Count);
            Assert.IsTrue(reinterpreted.ContainsKey(1));
        }

        [Test]
        public void Intersect_KeepsOnlySharedKeys()
        {
            using var a = new ArrayMapNative<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapNative<int, int>(8, Allocator.Temp);

            a.Add(1, 1);
            a.Add(2, 2);
            a.Add(3, 3);

            b.Add(2, 0);
            b.Add(3, 0);
            b.Add(4, 0);

            a.Intersect(in b);

            Assert.AreEqual(2, a.Count);
            Assert.IsFalse(a.ContainsKey(1));
            Assert.IsTrue(a.ContainsKey(2));
            Assert.IsTrue(a.ContainsKey(3));
        }

        [Test]
        public void Exclude_RemovesSharedKeys()
        {
            using var a = new ArrayMapNative<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapNative<int, int>(8, Allocator.Temp);

            a.Add(1, 1);
            a.Add(2, 2);
            a.Add(3, 3);

            b.Add(2, 0);
            b.Add(3, 0);

            a.Exclude(in b);

            Assert.AreEqual(1, a.Count);
            Assert.IsTrue(a.ContainsKey(1));
            Assert.IsFalse(a.ContainsKey(2));
            Assert.IsFalse(a.ContainsKey(3));
        }

        [Test]
        public void Union_MergesAndOverwrites()
        {
            using var a = new ArrayMapNative<int, int>(8, Allocator.Temp);
            using var b = new ArrayMapNative<int, int>(8, Allocator.Temp);

            a.Add(1, 1);
            a.Add(2, 2);

            b.Add(2, 200);
            b.Add(3, 300);

            a.Union(in b);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, a[1]);
            Assert.AreEqual(200, a[2]);
            Assert.AreEqual(300, a[3]);
        }

        [Test]
        public void AsReadOnly_ReflectsContent()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            var readOnly = map.AsReadOnly();

            Assert.IsTrue(readOnly.IsCreated);
            Assert.AreEqual(2, readOnly.Count);
            Assert.AreEqual(10, readOnly[1]);
            Assert.IsTrue(readOnly.ContainsKey(2));
            Assert.IsTrue(readOnly.TryGetValue(2, out var value));
            Assert.AreEqual(20, value);
            Assert.IsFalse(readOnly.ContainsKey(99));
        }

        [Test]
        public void AsReadOnly_TryGetValue_TypeWrap_ReturnsExpected()
        {
            using var map = new ArrayMapNative<UintId, uint>(4, Allocator.Temp);
            map.Add(1, 10);
            map.Add(2, 20);

            Asserts(map.AsReadOnly());

            static void Asserts(ArrayMapNative<UintId, uint>.ReadOnly readOnly)
            {
                Assert.IsTrue(readOnly.TryGetValue(1, out var value1));
                Assert.AreEqual(10, value1);

                Assert.IsTrue(readOnly.TryGetValue(2, out var value2));
                Assert.AreEqual(20, value2);

                Assert.IsFalse(readOnly.TryGetValue(3, out var missing));
                Assert.AreEqual(0, missing);
            }
        }

        [Test]
        public void AsReadOnly_Enumerator_VisitsAllPairs()
        {
            using var map = new ArrayMapNative<int, int>(8, Allocator.Temp);

            for (var i = 0; i < 4; i++)
            {
                map.Add(i, i * 10);
            }

            var seen = new Dictionary<int, int>();

            foreach (var pair in map.AsReadOnly())
            {
                seen[pair.Key] = pair.Value;
            }

            Assert.AreEqual(4, seen.Count);

            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(i * 10, seen[i]);
            }
        }

        [Test]
        public void Dispose_MarksNotCreated()
        {
            var map = new ArrayMapNative<int, int>(4, Allocator.Temp);
            Assert.IsTrue(map.IsCreated);

            map.Dispose();

            Assert.IsFalse(map.IsCreated);
        }

        [Test]
        public void Collisions_ManyKeys_AllRetrievable()
        {
            const int N = 500;
            using var map = new ArrayMapNative<int, int>(2, Allocator.Temp);

            for (var i = 0; i < N; i++)
            {
                map.Add(i, i);
            }

            for (var i = 0; i < N; i++)
            {
                Assert.IsTrue(map.TryGetValue(i, out var value), $"missing key {i}");
                Assert.AreEqual(i, value);
            }
        }
    }
}

#endif
