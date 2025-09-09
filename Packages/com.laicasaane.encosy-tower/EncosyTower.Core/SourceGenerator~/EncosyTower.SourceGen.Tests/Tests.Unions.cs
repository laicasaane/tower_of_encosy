using EncosyTower.Unions;
using UnityEngine;

namespace EncosyTower.Tests.UnionTests
{
    public readonly partial struct Vector3Union : IUnion<Vector3> { }

    public struct StructWithRefs
    {
        public int intValue;
        public object objValue;
    }

    public readonly partial struct StructWithRefsUnion : IUnion<StructWithRefs> { }

    public class UnionTest
    {
        public void DoSomething()
        {
            var _ = Union<Vector2>.GetConverter();
        }
    }
}
