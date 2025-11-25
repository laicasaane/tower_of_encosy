using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.TypeWraps;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.TypeWraps
{
    [WrapRecord]
    public readonly partial record struct HeroId(int _);

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ArrayAttribute : Attribute
    {
        public ArrayAttribute(int[] values) { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ValueAttribute : Attribute
    {
        public ValueAttribute(string strValue, AttributeTargets enumValue, Type type) { }
    }

    public struct MyPrinter
    {
        public readonly void PrintArray([Array(new[] { 1, 2, 3 })] int[] values) { }

        public readonly void Print([Value("a", AttributeTargets.Method, typeof(int))] object value) { }
    }

    [WrapRecord]
    public partial record struct MyPrinterNext(MyPrinter _)
    {
        readonly void Do()
        {
            PrintArray(default);
            Print(default);
        }
    }

    [WrapType(typeof(List<int>))]
    public partial class ListInt
    {

    }

    [WrapType(typeof(VisualElement))]
    public readonly partial struct VisualElementWrapper { }

    public struct V1 : IEquatable<V1>, IComparable<V1>, IComparable
    {
        public int value;

        public readonly int CompareTo(V1 other)
            => value.CompareTo(other.value);

        public readonly int CompareTo(object obj)
            => obj is V1 other ? value.CompareTo(other.value) : 1;

        public readonly override bool Equals([NotNullWhen(true)] object obj)
            => obj is V1 other && other.value == value;

        public readonly bool Equals(V1 other)
            => value == other.value;

        public readonly override int GetHashCode()
            => value.GetHashCode();
    }

    [WrapRecord]
    public readonly partial record struct V1x(V1 _)
    {
        // readonly cannot set
        // record property cannot set
    }

    [WrapType(typeof(V1))]
    public readonly partial struct V1y
    {

    }

    [WrapType(typeof(V1))]
    public partial struct V1z
    {

    }

    public record struct A(int[] X)
    {
        private Vector2 _y;

        public int First
        {
            get => X[0];
            set => X[0] = value;
        }

        public float Y
        {
            get => _y.y;
            set => _y.y = value;
        }
    }

    public readonly struct B
    {
        private readonly A _a;

        public int First
        {
            get => _a.X[0];
        }

        public float Y
        {
            get => _a.Y;
            //set => _a.Y = value;
        }
    }

    [WrapType(typeof(A))]
    public partial struct C
    {

    }

    [WrapType(typeof(A))]
    public readonly partial struct D
    {

    }

    [WrapRecord]
    public readonly partial record struct E(A _)
    {
        public int First
        {
            get => _.X[0];
            set => _.X[0] = value;
        }
    }

    [WrapType(typeof(int), nameof(Raw))]
    public readonly partial struct F
    {
        public readonly int Raw;
    }

    [WrapRecord]
    public readonly partial record struct Position(float3 _);

    [WrapRecord]
    public readonly partial record struct EnumWrapper(AttributeTargets _);

    [WrapRecord]
    public unsafe readonly partial record struct WrappedNativeList<T>(NativeList<T> _)
        where T : unmanaged
    { }
}
