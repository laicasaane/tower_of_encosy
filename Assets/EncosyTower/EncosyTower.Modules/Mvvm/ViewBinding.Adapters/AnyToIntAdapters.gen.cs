/// <auto-generated>
///**********************************************************///
///                                                          ///
/// This file is auto-generated by AnyToIntAdaptersGenerator ///
///                DO NOT manually modify it!                ///
///                                                          ///
///**********************************************************///
/// </auto-generated>

#pragma warning disable

using System;
using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Byte ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(byte), destType: typeof(int), order: 0)]
    public sealed class ByteToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out byte result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("SByte ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(sbyte), destType: typeof(int), order: 0)]
    public sealed class SByteToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out sbyte result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Char ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(char), destType: typeof(int), order: 0)]
    public sealed class CharToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out char result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Double ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(double), destType: typeof(int), order: 0)]
    public sealed class DoubleToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out double result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Float ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(int), order: 0)]
    public sealed class FloatToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Int ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(int), destType: typeof(int), order: 0)]
    public sealed class IntToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out int result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("UInt ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(uint), destType: typeof(int), order: 0)]
    public sealed class UIntToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out uint result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Long ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(long), destType: typeof(int), order: 0)]
    public sealed class LongToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out long result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("ULong ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(ulong), destType: typeof(int), order: 0)]
    public sealed class ULongToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out ulong result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Short ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(short), destType: typeof(int), order: 0)]
    public sealed class ShortToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out short result))
            {
                return (int)result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("UShort ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(ushort), destType: typeof(int), order: 0)]
    public sealed class UShortToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out ushort result))
            {
                return (int)result;
            }

            return union;
        }
    }

}
