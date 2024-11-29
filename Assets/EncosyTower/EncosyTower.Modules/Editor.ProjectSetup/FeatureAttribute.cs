using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Modules.Editor.ProjectSetup
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class FeatureAttribute : Attribute
    {
        public string Name { get; }

        public FeatureAttribute([NotNull] string name)
        {
            Name = name;
        }
    }
}
