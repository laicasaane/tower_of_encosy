using System;
using System.Diagnostics;

namespace EncosyTower.Core
{
    /// <summary>
    /// The API marked with this attribute should take effect only in authoring contexts
    /// (such as data conversion from CSV to ScriptableObject)
    /// even though it is defined in non-authoring assemblies (such as EncosyTower.Core).
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR"), Conditional("ENCOSY_INCLUDE_AUTHORING")]
    public sealed class ApiForAuthoringAttribute : Attribute
    {
    }
}
