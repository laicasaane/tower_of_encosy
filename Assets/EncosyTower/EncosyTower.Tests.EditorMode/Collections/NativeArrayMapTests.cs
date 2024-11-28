using EncosyTower.Modules.Collections;
using NUnit.Framework;
using Unity.Collections;

namespace EncosyTower.Tests.Collections
{
    public class NativeArrayMapTests
    {
        [Test]
        public void NativeArrayMap_Tests()
        {
            var map = new NativeArrayMap<int, float>(12, Allocator.Temp);

            map.Add(1, 5f);
            map.Add(2, 6f);
            map.Add(3, 7f);

            Assert.IsTrue(map.Count == 3);
            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsTrue(map.ContainsKey(2));
            Assert.IsTrue(map.ContainsKey(3));
            Assert.IsTrue(map.ContainsKey(4) == false);

            Assert.IsTrue(map[1] == 5f);
            Assert.IsTrue(map[2] == 6f);
            Assert.IsTrue(map[3] == 7f);

            map.Remove(2);

            Assert.IsTrue(map.Count == 2);
            Assert.IsTrue(map.ContainsKey(2) == false);
            Assert.IsTrue(map[1] == 5f);
            Assert.IsTrue(map[3] == 7f);

            map.Clear();

            Assert.IsTrue(map.Count == 0);
        }
    }
}
