// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.Modules.SourceGen
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
        public static readonly DiagnosticDescriptor InvalidPropertyTargetedAttributeOnObservableProperty = new DiagnosticDescriptor(
              id: "MVVMTK0001"
            , title: "Invalid property targeted attribute type"
            , messageFormat: "The field {0} annotated with [ObservableProperty] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "ObservablePropertyGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated property for a field annotated with [ObservableProperty] must correctly be resolved to valid types."
        );

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a property with <c>[ObservableProperty]</c> is using an invalid attribute targeting the property.
        /// <para>
        /// Format: <c>"The property {0} annotated with [ObservableProperty] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
        /// </para>
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFieldMethodTargetedAttributeOnObservableProperty = new DiagnosticDescriptor(
              id: "MVVMTK0002"
            , title: "Invalid field targeted attribute type"
            , messageFormat: "The property {0} annotated with [ObservableProperty] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "ObservablePropertyGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated field or method for a property annotated with [ObservableProperty] must correctly be resolved to valid types."
        );

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a method with <c>[RelayCommand]</c> is using an invalid attribute targeting the field or property.
        /// <para>
        /// Format: <c>"The method {0} annotated with [RelayCommand] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
        /// </para>
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFieldOrPropertyTargetedAttributeOnRelayCommandMethod = new DiagnosticDescriptor(
              id: "MVVMTK0003"
            , title: "Invalid field targeted attribute type"
            , messageFormat: "The method {0} annotated with [RelayCommand] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "RelayCommandGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated field or property for a method annotated with [RelayCommand] must correctly be resolved to valid types."
        );

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a method with <c>[BindingProperty]</c> is using an invalid attribute targeting the field.
        /// <para>
        /// Format: <c>"The method {0} annotated with [BindingProperty] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
        /// </para>
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFieldTargetedAttributeOnBindingPropertyMethod = new DiagnosticDescriptor(
              id: "MVVMTK0004"
            , title: "Invalid field targeted attribute type"
            , messageFormat: "The method {0} annotated with [BindingProperty] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "BinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated field for a method annotated with [BindingProperty] must correctly be resolved to valid types."
        );

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> indicating when a method with <c>[BindingCommand]</c> is using an invalid attribute targeting the field.
        /// <para>
        /// Format: <c>"The method {0} annotated with [BindingCommand] is using attribute "{1}" which was not recognized as a valid type (are you missing a using directive?)"</c>.
        /// </para>
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFieldTargetedAttributeOnBindingCommandMethod = new DiagnosticDescriptor(
              id: "MVVMTK0005"
            , title: "Invalid field targeted attribute type"
            , messageFormat: "The method {0} annotated with [BindingCommand] is using attribute \"{1}\" which was not recognized as a valid type (are you missing a using directive?)"
            , category: "BinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "All attributes targeting the generated field for a method annotated with [BindingCommand] must correctly be resolved to valid types."
        );
    }
}
