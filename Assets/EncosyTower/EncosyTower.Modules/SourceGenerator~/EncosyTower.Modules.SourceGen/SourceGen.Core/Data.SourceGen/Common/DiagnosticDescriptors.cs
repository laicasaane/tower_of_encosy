// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.Modules.Data.SourceGen
{
    /// <summary>
    /// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
    /// </summary>
    internal static class DiagnosticDescriptors
    {
        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a field with <c>[ObservableProperty]</c> is using an invalid attribute targeting the property.
        /// <para>
        /// Format: <c>"The field {0} annotated with [ObservableProperty] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
        /// </para>
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidPropertyTargetedAttribute = new DiagnosticDescriptor(
              id: "DATA_0001"
            , title: "Invalid property targeted attribute type"
            , messageFormat: "The field \"{0}\" is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated property for a field must correctly be resolved to valid types."
        );

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a property with <c>[ObservableProperty]</c> is using an invalid attribute targeting the property.
        /// <para>
        /// Format: <c>"The property {0} annotated with [ObservableProperty] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
        /// </para>
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFieldTargetedAttribute = new DiagnosticDescriptor(
              id: "DATA_0002"
            , title: "Invalid field targeted attribute type"
            , messageFormat: "The property \"{0}\" is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated field for a property must correctly be resolved to valid types."
        );

        public static readonly DiagnosticDescriptor CannotDecorateImmutableDataWithFieldPolicyAttribute = new DiagnosticDescriptor(
              id: "DATA_0003"
            , title: "Cannot decorate immutable data with [DataFieldPolicy] attribute"
            , messageFormat: "\"{0}\" is immutable thus cannot be decorated with [DataFieldPolicy] attribute (are you missing [DataMutable] attribute?)"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The data type must already decorated with [DataMutable] to be able have [DataFieldPolicy]."
        );

        public static readonly DiagnosticDescriptor ImmutableDataFieldMustBePrivate = new DiagnosticDescriptor(
              id: "DATA_0004"
            , title: "Fields of immutable data must be private"
            , messageFormat: "\"{0}\" is immutable thus its fields must be private (are you missing [DataMutable] attribute?)"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The data type must already decorated with [DataMutable] to be able have non-private fields."
        );

        public static readonly DiagnosticDescriptor OnlyPrivateOrInitOnlySetterIsAllowed = new DiagnosticDescriptor(
              id: "DATA_0005"
            , title: "Only private setter or init-only setter is allowed because the type is either immutable or decorated with [DataMutable(withoutPropertySetter: true)]"
            , messageFormat: "Only private setter or init-only setter is allowed because \"{0}\" is either immutable or decorated with [DataMutable(withoutPropertySetter: true)]"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Use private setter, or init-only setter, or decorate the type with [DataMutable(withoutPropertySetter: false)]."
        );

        public static readonly DiagnosticDescriptor CollectionIsNotApplicableForProperty = new DiagnosticDescriptor(
              id: "DATA_0006"
            , title: "Collection type is not applicable for the property"
            , messageFormat: "Type \"{0}\" is a collection thus it is not applicable for the \"{1}\" property"
            , category: "DataGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Collection type is not applicable for the property."
        );

        public static readonly DiagnosticDescriptor MustBeApplicableForTypeArgument = new DiagnosticDescriptor(
              id: "DATA_0010"
            , title: "Must be either a struct, a class or an enum to replace type argument"
            , messageFormat: "Type \"{0}\" is not applicable to replace \"{1}\", must be either a struct, a class or an enum"
            , category: "DataTableAssetGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Must be either a struct, a class or an enum."
        );
    }
}
