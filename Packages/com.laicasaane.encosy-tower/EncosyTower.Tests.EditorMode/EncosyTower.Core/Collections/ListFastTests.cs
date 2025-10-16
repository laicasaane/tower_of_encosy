using System.Collections.Generic;
using EncosyTower.Collections;
using NUnit.Framework;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public class ListFastTests
    {
        [Test]
        public void ListFast_Tests()
        {
            var list = new ListFast<int>(new List<int>(4));

            Assert.AreEqual(true, list.IsCreated);
            Assert.AreEqual(4, list.Capacity);
            Assert.AreEqual(0, list.Count);

            list.Add(5);
            list.Add(8);
            list.Add(2);
            list.Add(0);

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(5, list[0]);
            Assert.AreEqual(8, list[1]);
            Assert.AreEqual(2, list[2]);
            Assert.AreEqual(0, list[3]);

            list.Remove(8);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2, list[1]);
        }
    }
}
