#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.SystemExtensions;
using Unity.Collections;
using UnityEditor;

namespace EncosyTower.Editor.Common
{
    public static class SerializableGuidEditorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCopyFrom(ref this Guid self, SerializedProperty property)
        {
            SerializableGuid serializable = default;

            if (TryCopyFrom(ref serializable, property) == false)
            {
                return false;
            }

            self = serializable;
            return true;
        }

        public static bool TryCopyFrom(ref this SerializableGuid self, SerializedProperty property)
        {
            if (TryDetermineBytesProperty(ref property) == false)
            {
                self = default;
                return false;
            }

            Span<byte> bytes = stackalloc byte[SerializableGuid.SIZE];

            for (var i = 0; i < SerializableGuid.SIZE; i++)
            {
                bytes[i] = (byte)GetByteProperty(property, i).intValue;
            }

            self = new SerializableGuid(bytes);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCopyTo(in this Guid self, SerializedProperty property)
            => TryCopyTo(self.AsSerializable(), property);

        public static bool TryCopyTo(in this SerializableGuid self, SerializedProperty property)
        {
            if (TryDetermineBytesProperty(ref property) == false)
            {
                return false;
            }

            var bytes = self.AsReadOnlySpan();

            for (var i = 0; i < SerializableGuid.SIZE; i++)
            {
                GetByteProperty(property, i).intValue = bytes[i];
            }

            return true;
        }

        private static bool TryDetermineBytesProperty(ref SerializedProperty property)
        {
            if (property.type == nameof(SerializableGuid))
            {
                property = property.FindPropertyRelative(nameof(SerializableGuid._bytes));
                return true;
            }

            return false;
        }

        private static SerializedProperty GetByteProperty(SerializedProperty property, int index)
        {
            var name = index switch {
                00 => nameof(FixedBytes16.byte0000),
                01 => nameof(FixedBytes16.byte0001),
                02 => nameof(FixedBytes16.byte0002),
                03 => nameof(FixedBytes16.byte0003),
                04 => nameof(FixedBytes16.byte0004),
                05 => nameof(FixedBytes16.byte0005),
                06 => nameof(FixedBytes16.byte0006),
                07 => nameof(FixedBytes16.byte0007),
                08 => nameof(FixedBytes16.byte0008),
                09 => nameof(FixedBytes16.byte0009),
                10 => nameof(FixedBytes16.byte0010),
                11 => nameof(FixedBytes16.byte0011),
                12 => nameof(FixedBytes16.byte0012),
                13 => nameof(FixedBytes16.byte0013),
                14 => nameof(FixedBytes16.byte0014),
                15 => nameof(FixedBytes16.byte0015),
                _  => throw new ArgumentOutOfRangeException(nameof(index)),
            };

            return property.FindPropertyRelative(name);
        }
    }
}

#endif
