using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Tests.VariantTests
{
    public readonly partial struct Vector3Variant : IVariant<Vector3> { }

    public struct StructWithRefs
    {
        public int intValue;
        public object objValue;
    }

    public readonly partial struct StructWithRefsVariant : IVariant<StructWithRefs> { }

    public class VariantTest
    {
        public void DoSomething()
        {
            var _ = Variant<Vector2>.GetConverter();
        }
    }
}
