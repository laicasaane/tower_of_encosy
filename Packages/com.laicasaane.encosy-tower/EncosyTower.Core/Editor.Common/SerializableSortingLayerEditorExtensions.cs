#if UNITY_EDITOR

using EncosyTower.UnityExtensions;
using UnityEditor;

namespace EncosyTower.Editor.Common
{
    public static class SerializableSortingLayerEditorExtensions
    {
        public static bool TryCopyFrom(ref this SerializableSortingLayer self, SerializedProperty property)
        {
            if (TryGetValueProperty(ref property) == false)
            {
                self = default;
                return false;
            }

            self = new SerializableSortingLayer(property.intValue);
            return true;
        }

        public static bool TryCopyTo(in this SerializableSortingLayer self, SerializedProperty property)
        {
            if (TryGetValueProperty(ref property) == false)
            {
                return false;
            }

            property.intValue = self.value;
            return true;
        }

        private static bool TryGetValueProperty(ref SerializedProperty property)
        {
            if (property.type == nameof(SerializableSortingLayer))
            {
                property = property.FindPropertyRelative(nameof(SerializableSortingLayer.value));
                return property != null;
            }

            return false;
        }
    }
}

#endif
