#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.SystemExtensions;
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
            if (TryGetBytesProperty(ref property) == false)
            {
                self = default;
                return false;
            }

            Span<byte> bytes = stackalloc byte[SerializableGuid.SIZE];

            for (var i = 0; i < SerializableGuid.SIZE; i++)
            {
                bytes[i] = (byte)property.GetFixedBufferElementAtIndex(i).intValue;
            }

            self = new SerializableGuid(bytes);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCopyTo(in this Guid self, SerializedProperty property)
            => TryCopyTo(self.AsSerializable(), property);

        public static bool TryCopyTo(in this SerializableGuid self, SerializedProperty property)
        {
            if (TryGetBytesProperty(ref property) == false)
            {
                return false;
            }

            var bytes = self.AsReadOnlySpan();

            for (var i = 0; i < SerializableGuid.SIZE; i++)
            {
                property.GetFixedBufferElementAtIndex(i).intValue = bytes[i];
            }

            return true;
        }

        private static bool TryGetBytesProperty(ref SerializedProperty property)
        {
            if (property.type == nameof(SerializableGuid))
            {
                property = property.FindPropertyRelative(nameof(SerializableGuid._bytes));
                return property != null;
            }

            return false;
        }
    }
}

#endif
