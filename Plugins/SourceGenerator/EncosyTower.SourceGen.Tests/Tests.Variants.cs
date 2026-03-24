using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Tests.VariantTests
{
    [Variant(typeof(Vector3))]
    public readonly partial struct Vector3Variant { }

    public struct StructWithRefs
    {
        public int intValue;
        public object objValue;
    }

    [Variant(typeof(StructWithRefs))]
    public readonly partial struct StructWithRefsVariant { }

    public class Person { }

    [Variant(typeof(Person))]
    public readonly partial struct PersonVariant { }

    public class VariantTest
    {
        public void DoSomething()
        {
            var _ = Variant<Vector2>.GetConverter();
        }
    }
}
