using System;

namespace EncosyTower.Annotations
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class RemoveFromDocsAttribute : Attribute { }
}
