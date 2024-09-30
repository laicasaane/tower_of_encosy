using Module.Core.Collections;
using NUnit.Framework;

namespace Tests.Module.Core
{
    public class ArrayMapTests
    {
        [Test]
        public void ArrayMap_Tests()
        {
            var map = new ArrayMap<int, string>();

            map.Add(1, "One");
            map.Add(2, "Two");
            map.Add(3, "Three");

            Assert.IsTrue(map.Count == 3);
            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsTrue(map.ContainsKey(2));
            Assert.IsTrue(map.ContainsKey(3));
            Assert.IsTrue(map.ContainsKey(4) == false);

            Assert.IsTrue(map[1] == "One");
            Assert.IsTrue(map[2] == "Two");
            Assert.IsTrue(map[3] == "Three");

            map.Remove(2);

            Assert.IsTrue(map.Count == 2);
            Assert.IsTrue(map.ContainsKey(2) == false);
            Assert.IsTrue(map[1] == "One");
            Assert.IsTrue(map[3] == "Three");

            map.Clear();

            Assert.IsTrue(map.Count == 0);
        }
    }
}
