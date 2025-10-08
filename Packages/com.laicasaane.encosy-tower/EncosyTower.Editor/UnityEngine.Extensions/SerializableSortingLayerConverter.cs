#if UNITY_EDITOR

using EncosyTower.UnityExtensions;
using UnityEditor.UIElements;

namespace EncosyTower.Editor.UnityExtensions
{
    public class SerializableSortingLayerConverter : UxmlAttributeConverter<SerializableSortingLayer>
    {
        public override SerializableSortingLayer FromString(string value)
        {
            return new SerializableSortingLayer(value);
        }

        public override string ToString(SerializableSortingLayer value)
        {
            return value.Name;
        }
    }
}

#endif
