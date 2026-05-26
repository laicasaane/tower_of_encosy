using System;

namespace EncosyTower.Core
{
    /// <summary>
    /// The API marked with this attribute should take effect only in authoring contexts
    /// (such as data conversion from CSV to ScriptableObject)
    /// even though it is defined in non-authoring assemblies (such as EncosyTower.Core).
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ApiForAuthoringAttribute : Attribute
    {
    }
}
