using System;
using EncosyTower.Common;
using NUnit.Framework;

namespace EncosyTower.Tests.EncosyTower.Common
{
    public class SerializableGuidTests
    {
        [Test]
        public void SerializableGuid_Test_Implicit_Operator()
        {
            var guid = new Guid("b23b48fcb15d994c939451ce1a44b3fd");
            SerializableGuid serializedGuid = guid;
            Assert.IsTrue(serializedGuid == guid);
        }

#if UNITY_COLLECTIONS
        [Test]
        public void SerializableGuid_Test_Conversion_D()
        {
            const string STR = "b23b48fc-b15d-994c-9394-51ce1a44b3fd";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void SerializableGuid_Test_Conversion_N()
        {
            const string STR = "b23b48fcb15d994c939451ce1a44b3fd";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void SerializableGuid_Test_Conversion_B()
        {
            const string STR = "{b23b48fc-b15d-994c-9394-51ce1a44b3fd}";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void SerializableGuid_Test_Conversion_X()
        {
            const string STR = "{0xb23b48fc,0xb15d,0x994c,{0x93,0x94,0x51,0xce,0x1a,0x44,0xb3,0xfd}}";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }
#endif
    }
}
