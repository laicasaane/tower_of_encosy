using System;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    public struct PropertyBindingSpec : IEquatable<PropertyBindingSpec>
    {
        public string memberName;
        public string memberPascalName;
        public string propFullTypeName;
        public bool needsInModifier;
        public string setterMethod;
        public string label;
        public string setterMethodName;
        public string generatedClassName;
        public bool skipGeneration;
        public bool isObsolete;
        public string obsoleteMessage;
        public string variantConverterPropertyName;
        public bool useCustomSetter;
        public string customSetterPartialMethodName;

        public readonly bool Equals(PropertyBindingSpec other)
            => string.Equals(memberName, other.memberName, StringComparison.Ordinal)
            && string.Equals(propFullTypeName, other.propFullTypeName, StringComparison.Ordinal)
            && needsInModifier == other.needsInModifier
            && string.Equals(setterMethod, other.setterMethod, StringComparison.Ordinal)
            && string.Equals(label, other.label, StringComparison.Ordinal)
            && string.Equals(setterMethodName, other.setterMethodName, StringComparison.Ordinal)
            && skipGeneration == other.skipGeneration
            && isObsolete == other.isObsolete
            && string.Equals(variantConverterPropertyName, other.variantConverterPropertyName, StringComparison.Ordinal)
            && useCustomSetter == other.useCustomSetter
            && string.Equals(customSetterPartialMethodName, other.customSetterPartialMethodName, StringComparison.Ordinal);

        public readonly override bool Equals(object obj)
            => obj is PropertyBindingSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  memberName
                , propFullTypeName
                , needsInModifier
                , setterMethod
                , label
                , setterMethodName
                , skipGeneration
                , isObsolete
            )
            .Add(variantConverterPropertyName)
            .Add(useCustomSetter)
            .Add(customSetterPartialMethodName);
    }
}
