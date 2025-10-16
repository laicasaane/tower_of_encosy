using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Collections.Extensions.Unsafe;
using NUnit.Framework;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public class FasterListTests
    {
        [Test]
        public void FasterList_ProxiedList_Tests()
        {
            var fasterList = new FasterList<int>(4);
            var proxiedList = fasterList.ToProxiedList();

            using (var synchronizer = proxiedList.GetListUnsafe(out var list))
            {
                Assert.AreEqual(true, proxiedList.IsCreated);
                Assert.AreEqual(4, proxiedList.Capacity);
                Assert.AreEqual(0, proxiedList.Count);

                proxiedList.Add(5);
                proxiedList.Add(8);
                proxiedList.Add(2);
                proxiedList.Add(0);

                Assert.AreEqual(4, fasterList.Count);
                Assert.AreEqual(5, fasterList[0]);
                Assert.AreEqual(8, fasterList[1]);
                Assert.AreEqual(2, fasterList[2]);
                Assert.AreEqual(0, fasterList[3]);

                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(5, list[0]);
                Assert.AreEqual(8, list[1]);
                Assert.AreEqual(2, list[2]);
                Assert.AreEqual(0, list[3]);

                proxiedList.Remove(8);

                Assert.AreEqual(3, fasterList.Count);
                Assert.AreEqual(2, fasterList[1]);

                Assert.AreEqual(3, list.Count);
                Assert.AreEqual(2, list[1]);

                list.Add(22);
                list.Add(33);

                Assert.AreNotEqual(5, fasterList.Count);

                synchronizer.Synchronize();

                Assert.AreEqual(5, fasterList.Count);
                Assert.AreEqual(22, fasterList[3]);
                Assert.AreEqual(33, fasterList[4]);
            }
        }
    }
}
