#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Modules.Editor.ProjectSetup
{
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

#endif
