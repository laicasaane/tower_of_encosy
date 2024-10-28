/// <auto-generated>
///***********************************************************///
///                                                           ///
/// This file is auto-generated by AnyToBoolAdaptersGenerator ///
///                DO NOT manually modify it!                 ///
///                                                           ///
///***********************************************************///
/// </auto-generated>

#pragma warning disable

using System;
using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Byte ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(byte), destType: typeof(bool), order: 0)]
    public sealed class ByteToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out byte result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("SByte ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(sbyte), destType: typeof(bool), order: 0)]
    public sealed class SByteToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out sbyte result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Char ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(char), destType: typeof(bool), order: 0)]
    public sealed class CharToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out char result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Double ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(double), destType: typeof(bool), order: 0)]
    public sealed class DoubleToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out double result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Float ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(bool), order: 0)]
    public sealed class FloatToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Int ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(int), destType: typeof(bool), order: 0)]
    public sealed class IntToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out int result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("UInt ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(uint), destType: typeof(bool), order: 0)]
    public sealed class UIntToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out uint result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Long ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(long), destType: typeof(bool), order: 0)]
    public sealed class LongToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out long result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("ULong ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(ulong), destType: typeof(bool), order: 0)]
    public sealed class ULongToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out ulong result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Short ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(short), destType: typeof(bool), order: 0)]
    public sealed class ShortToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out short result))
            {
                return result > 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("UShort ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(ushort), destType: typeof(bool), order: 0)]
    public sealed class UShortToBoolAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out ushort result))
            {
                return result > 0;
            }

            return union;
        }
    }

}
