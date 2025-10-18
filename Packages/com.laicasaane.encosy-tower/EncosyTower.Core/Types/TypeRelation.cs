#pragma warning disable IDE0060 // Remove unused parameter

using System.Runtime.CompilerServices;

namespace EncosyTower.Types
{
    public readonly struct TypeRelation<T1>
    {
        public TypeId<T1> Id1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T1>.Id;
        }
    }

    public readonly struct TypeRelation<T1, T2>
    {
        public TypeId<T1> Id1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T1>.Id;
        }

        public TypeId<T2> Id2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T2>.Id;
        }
    }

    public readonly struct TypeRelation<T1, T2, T3>
    {
        public TypeId<T1> Id1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T1>.Id;
        }

        public TypeId<T2> Id2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T2>.Id;
        }

        public TypeId<T3> Id3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T3>.Id;
        }
    }

    public readonly struct TypeRelation<T1, T2, T3, T4>
    {
        public TypeId<T1> Id1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T1>.Id;
        }

        public TypeId<T2> Id2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T2>.Id;
        }

        public TypeId<T3> Id3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T3>.Id;
        }

        public TypeId<T4> Id4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T4>.Id;
        }
    }

    public static class TypeTypeRelationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1> GetTypeRelation<T1>(this T1 t1)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1, T2> GetTypeRelation<T1, T2>(this T1 t1, T2 t2)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1, T2, T3> GetTypeRelation<T1, T2, T3>(this T1 t1, T2 t2, T3 t3)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1, T2, T3, T4> GetTypeRelation<T1, T2, T3, T4>(this T1 t1, T2 t2, T3 t3, T4 t4)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1, T2> RelatesTo<T1, T2>(this TypeRelation<T1> self, T2 _)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1, T2, T3> RelatesTo<T1, T2, T3>(this TypeRelation<T1, T2> self, T3 _)
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeRelation<T1, T2, T3, T4> RelatesTo<T1, T2, T3, T4>(this TypeRelation<T1, T2, T3> self, T4 _)
            => default;
    }
}
