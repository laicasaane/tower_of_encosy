#if UNITY_NEWTONSOFT_JSON

using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class NewtonsoftJsonAotHelperAttribute : Attribute
    {
        public Type BaseType { get; }

        public NewtonsoftJsonAotHelperAttribute([NotNull] Type baseType)
        {
            BaseType = baseType;
        }
    }
}

#endif
