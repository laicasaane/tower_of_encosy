#if UNITY_EDITOR

using EncosyTower.Core;
using EncosyTower.UnityExtensions;
using UnityEditor;

namespace EncosyTower.Editor.Common
{
    [ApiForEditor]
    public static class SerializableSortingLayerEditorExtensions
    {
        [ApiForEditor]
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

        [ApiForEditor]
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
