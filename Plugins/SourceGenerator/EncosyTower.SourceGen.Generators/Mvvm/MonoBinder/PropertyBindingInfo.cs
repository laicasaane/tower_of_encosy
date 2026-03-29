using System;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    public struct PropertyBindingInfo : IEquatable<PropertyBindingInfo>
    {
        public string memberName;           // "interactable"
        public string memberPascalName;     // "Interactable"
        public string propFullTypeName;     // "global::System.Boolean"
        public bool needsInModifier;
        public string setterMethod;         // "" (property assignment) or method name (e.g. "SetActive")
        public string label;                // "Interactable"
        public string setterMethodName;     // "SetInteractable"
        public string generatedClassName;   // "UnityUIButtonBindingInteractable"
        public bool skipGeneration;
        public bool isObsolete;
        public string obsoleteMessage;

        /// <summary>Pre-computed <c>propType.ToValidIdentifier().MakeFirstCharUpperCase()</c>
        /// used for the <c>_variantConverter{X}</c> field name.
        /// Empty when the property type is the <c>Variant</c> type itself.</summary>
        public string variantConverterPropertyName;

        /// <summary>When <see langword="true"/>, the generator emits
        /// <c>private static partial void {customSetterPartialMethodName}(ComponentType target, ValueType value);</c>
        /// on the outer binder class, and the inner binding body calls that partial instead of assigning
        /// the property directly or invoking <see cref="setterMethod"/>.</summary>
        public bool useCustomSetter;

        /// <summary>Name of the generated <c>private static partial void</c> method on the outer binder class,
        /// e.g. <c>Set_Active</c>. Non-empty only when <see cref="useCustomSetter"/> is <see langword="true"/>.</summary>
        public string customSetterPartialMethodName;

        public readonly bool Equals(PropertyBindingInfo other)
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
            => obj is PropertyBindingInfo other && Equals(other);

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
