using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Modules.Tests.UnionTests
{
    public readonly partial struct Vector3Union : IUnion<Vector3> { }

    public struct StructWithRefs
    {
        public int intValue;
        public object objValue;
    }

    public readonly partial struct StructWithRefsUnion : IUnion<StructWithRefs> { }
}
