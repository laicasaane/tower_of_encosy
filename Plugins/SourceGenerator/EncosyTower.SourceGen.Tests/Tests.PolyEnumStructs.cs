using System;


namespace EncosyTower.Tests.PolyEnumStructs
{
    using EncosyTower.PolyEnumStructs;

    [PolyEnumStruct]
    public partial struct Task
    {
        public partial struct A
        {
            public bool Value { get; set; }
        }

        public partial struct B { }
    }
}

namespace EncosyTower.Tests.PolyEnumStructs
{
    using System.ComponentModel;
    using EncosyTower.Annotations;
    using EncosyTower.PolyEnumStructs;
    using UnityEngine;

    public enum ColorEnumType
    {
        Red, Green, Blue
    }

    public static class ColorAPI
    {
        public static int Value;
    }

    [PolyEnumStruct(SortFieldsBySize = true, AutoEquatable = true, WithEnumExtensions = true)]
    public partial struct ColorEnum
    {
        public partial interface IEnumCase
        {
            [ReadOnly(true)]
            public bool IsValid => false;

            public bool TryGet(out int value)
            {
                value = default;
                return false;
            }
        }

        [ConstructEnumCaseFrom(1)]
        [ConstructEnumCaseFrom(ColorEnumType.Red)]
        [Label("RED")]
        public partial struct Red : IDisposable
        {
            public int value;

            public ref int Value => throw new NotImplementedException();

            public readonly ref int this[int i, float f] => throw new NotImplementedException();

            public int this[int i]
            {
                readonly get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void DoSomething(int value, in Vector2 position)
            {
                Debug.Log($"DoSomething: {value}, {position}");
            }

            public Vector3 Calculate(in Vector3 value)
            {
                return value * 3;
            }

            public bool TryGet(out int value)
            {
                value = 0;
                return false;
            }

            public readonly ref int GetValueByRef()
            {
                return ref ColorAPI.Value;
            }

            public readonly ref readonly int GetValueByRefRO()
            {
                return ref ColorAPI.Value;
            }
        }
        public partial struct Undefined
        {
            public bool capture;
        }

    }
}

namespace EncosyTower.Tests.PolyEnumStructs
{
    using EncosyTower.PolyEnumStructs;
    using UnityEngine;

    partial struct ColorEnum
    {
        [ConstructEnumCaseFrom(2)]
        [ConstructEnumCaseFrom(ColorEnumType.Green)]
        public partial struct Green : IDisposable
        {
            public int value;

            public ref int Value => throw new NotImplementedException();

            public readonly ref int this[int i, float f] => throw new NotImplementedException();

            public int this[int i]
            {
                readonly get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void DoSomething(int value, in Vector2 position)
            {
                Debug.Log($"DoSomething: {value}, {position}");
            }

            public Vector3 Calculate(in Vector3 value)
            {
                return value * 3;
            }

            public bool TryGet(out int value)
            {
                value = 0;
                return false;
            }

            public readonly ref int GetValueByRef()
            {
                return ref ColorAPI.Value;
            }

            public readonly ref readonly int GetValueByRefRO()
            {
                return ref ColorAPI.Value;
            }
        }
    }
}

namespace EncosyTower.Tests.PolyEnumStructs
{
    using EncosyTower.PolyEnumStructs;
    using UnityEngine;

    partial struct ColorEnum
    {
        [ConstructEnumCaseFrom("str")]
        public partial struct Blue : IDisposable
        {
            public float value;

            public ref int Value => throw new NotImplementedException();

            public readonly ref int this[int i, float f] => throw new NotImplementedException();

            public int this[int i]
            {
                readonly get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void DoSomething(int value, in Vector2 position)
            {
                Debug.Log($"DoSomething: {value}, {position}");
            }

            public Vector3 Calculate(in Vector3 value)
            {
                return value * 3;
            }

            public bool TryGet(out int value)
            {
                value = 0;
                return false;
            }

            public readonly ref int GetValueByRef()
            {
                return ref ColorAPI.Value;
            }

            public readonly ref readonly int GetValueByRefRO()
            {
                return ref ColorAPI.Value;
            }
        }
    }
}
