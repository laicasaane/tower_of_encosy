namespace Samples.PolyEnumStructs
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

namespace Samples.PolyEnumStructs
{
    using System;
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

        [EnumCaseValue(1)]
        [EnumCaseValue(ColorEnumType.Red)]
        [Label("RED")]
        public partial struct Red : IDisposable
        {
            public int value;

            public ref int Value => throw new NotImplementedException();

            public string Value2 { get; set; }

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

namespace Samples.PolyEnumStructs
{
    using System;
    using EncosyTower.PolyEnumStructs;
    using UnityEngine;

    partial struct ColorEnum
    {
        [EnumCaseValue(2)]
        [EnumCaseValue(ColorEnumType.Green)]
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

namespace Samples.PolyEnumStructs
{
    using System;
    using EncosyTower.PolyEnumStructs;
    using UnityEngine;

    partial struct ColorEnum
    {
        [EnumCaseValue("str")]
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

namespace Samples.PolyEnumStructs.Errors1
{
    using System.Runtime.InteropServices;
    using EncosyTower.PolyEnumStructs;
    using Unity.Collections;

    [PolyEnumStruct]
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct Error1
    {
        private static FixedString512Bytes InitFixedString(in FixedString32Bytes prefix)
        {
            FixedString512Bytes fs = default;

            if (prefix.IsEmpty == false)
            {
                fs.Append('[');
                fs.Append(prefix);
                fs.Append(']');
                fs.Append(' ');
            }

            return fs;
        }

        partial interface IEnumCase
        {
            FixedString512Bytes ToMessage(in FixedString32Bytes prefix);
        }

        public readonly partial struct Undefined
        {
            public FixedString512Bytes ToMessage(in FixedString32Bytes prefix)
            {
                FixedString128Bytes t = "An unknown error has occurred.";
                var fs = InitFixedString(prefix);
                fs.Append(t);
                return fs;
            }
        }

        public readonly partial struct ItemNotFound
        {
            public readonly int Id;

            public FixedString512Bytes ToMessage(in FixedString32Bytes prefix)
            {
                FixedString32Bytes t1 = "Item with ID ";
                FixedString32Bytes t2 = " was not found.";
                var fs = InitFixedString(prefix);
                fs.Append(t1);
                fs.Append(Id);
                fs.Append(t2);
                return fs;
            }
        }
    }
}

namespace Samples.PolyEnumStructs.Errors2
{
    using EncosyTower.PolyEnumStructs;
    using Unity.Collections;

    [PolyEnumStruct]
    public readonly partial struct Error2
    {
        private static FixedString512Bytes InitFixedString(in FixedString32Bytes prefix)
        {
            FixedString512Bytes fs = default;

            if (prefix.IsEmpty == false)
            {
                fs.Append('[');
                fs.Append(prefix);
                fs.Append(']');
                fs.Append(' ');
            }

            return fs;
        }

        partial interface IEnumCase
        {
            FixedString512Bytes ToMessage(in FixedString32Bytes prefix);
        }

        public readonly partial record struct Undefined
        {
            public FixedString512Bytes ToMessage(in FixedString32Bytes prefix)
            {
                FixedString128Bytes t = "An unknown error has occurred.";
                var fs = InitFixedString(prefix);
                fs.Append(t);
                return fs;
            }
        }

        public readonly partial record struct ItemNotFound(EncosyTower.Common.SerializableGuid Id)
        {
            public FixedString512Bytes ToMessage(in FixedString32Bytes prefix)
            {
                FixedString32Bytes t1 = "Item with ID ";
                FixedString32Bytes t2 = " was not found.";
                var fs = InitFixedString(prefix);
                fs.Append(t1);
                fs.Append(Id.ToFixedString());
                fs.Append(t2);
                return fs;
            }
        }

        public readonly partial record struct ItemAlreadyExists(int Id)
        {
            public FixedString512Bytes ToMessage(in FixedString32Bytes prefix)
            {
                FixedString32Bytes t1 = "Item with ID ";
                FixedString32Bytes t2 = " already exists.";
                var fs = InitFixedString(prefix);
                fs.Append(t1);
                fs.Append(Id);
                fs.Append(t2);
                return fs;
            }
        }

        [EnumCaseIgnore]
        public struct Data
        {
            public int intValue;
        }
    }
}

namespace Samples.PolyEnumStructs.Factories
{
    using EncosyTower.PolyEnumStructs;

    [PolyEnumStruct]
    public partial struct Activity
    {
        public partial record struct Shopping(int ItemCount, string Store);

        public partial struct Working
        {
            public string projectName;
            public int hoursPlanned;

            public Working(string projectName, int hoursPlanned)
            {
                this.projectName = projectName;
                this.hoursPlanned = hoursPlanned;
            }
        }

        public partial struct Idle
        {
            public string reason;
            public readonly int MaxMinutes;

            public Idle(string reason)
            {
                this.reason = reason;
            }

            public int Minutes { get; set; }
        }

        public partial struct Sleeping { }

        public partial struct Undefined
        {
            public string note;

            public int Code { get; set; }
        }
    }

    [PolyEnumFactoryFor(typeof(Activity))]
    public partial class ActivityFactory { }

    [PolyEnumFactoryFor(typeof(Activity))]
    public partial struct ActivityFactoryStruct
    {
        private readonly Activity _value;

        public ActivityFactoryStruct(Activity value)
        {
            _value = value;
        }
    }
}
