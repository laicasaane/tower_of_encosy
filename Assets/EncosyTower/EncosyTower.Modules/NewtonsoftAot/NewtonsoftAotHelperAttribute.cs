#if UNITY_NEWTONSOFT_JSON

using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Modules.NewtonsoftAot
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class NewtonsoftAotHelperAttribute : Attribute
    {
        public Type BaseType { get; }

        public NewtonsoftAotHelperAttribute([NotNull] Type baseType)
        {
            BaseType = baseType;
        }
    }
}

#endif
