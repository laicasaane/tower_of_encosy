#if UNITY_EDITOR

using System;
using EncosyTower.Common;
using UnityEditor.UIElements;

namespace EncosyTower.Editor.Common
{
    public class SerializableGuidConverter : UxmlAttributeConverter<SerializableGuid>
    {
        public override SerializableGuid FromString(string value)
        {
            return Guid.TryParse(value, out var result) ? result : Guid.Empty;
        }

        public override string ToString(SerializableGuid value)
        {
            return value.ToString();
        }
    }
}

#endif
