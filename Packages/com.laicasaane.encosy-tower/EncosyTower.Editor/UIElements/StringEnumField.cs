#if UNITY_EDITOR

using System.Collections;
using System.Reflection;
using EncosyTower.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    [UxmlElement]
    public partial class StringEnumField : PopupField<string>
    {
    }
}

#endif
