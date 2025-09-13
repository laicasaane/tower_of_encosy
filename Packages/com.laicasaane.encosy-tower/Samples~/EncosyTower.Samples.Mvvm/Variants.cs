using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    public readonly partial struct ColorVariant : IVariant<Color> { }

    public readonly partial struct Vector3Variant : IVariant<Vector3> { }

    public readonly partial struct QuaternionVariant : IVariant<Quaternion> { }
}
