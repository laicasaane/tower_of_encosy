using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    public readonly partial struct ColorUnion : IUnion<Color> { }

    public readonly partial struct Vector3Union : IUnion<Vector3> { }

    public readonly partial struct QuaternionUnion : IUnion<Quaternion> { }
}
