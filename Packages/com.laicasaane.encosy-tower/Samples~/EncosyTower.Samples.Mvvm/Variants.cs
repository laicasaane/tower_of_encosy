using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    [Variant(typeof(Color))]
    public readonly partial struct ColorVariant { }

    [Variant(typeof(Vector3))]
    public readonly partial struct Vector3Variant { }

    [Variant(typeof(Quaternion))]
    public readonly partial struct QuaternionVariant { }
}
