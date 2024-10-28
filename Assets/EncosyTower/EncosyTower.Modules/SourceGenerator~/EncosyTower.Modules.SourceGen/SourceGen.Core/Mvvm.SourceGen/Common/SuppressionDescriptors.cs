// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm
{
    /// <summary>
    /// A container for all <see cref="SuppressionDescriptors"/> instances for suppressed diagnostics by analyzers in this project.
    /// </summary>
    internal static class SuppressionDescriptors
    {
        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a field using [ObservableProperty] with an attribute list targeting a property.
        /// </summary>
        public static readonly SuppressionDescriptor PropertyAttributeListForObservableField = new(
              id: "MVVMTKSPR0001"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Fields using [ObservableProperty] can use [property:] or [method:] attribute lists to forward attributes to the generated properties and methods"
        );

        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a property using [ObservableProperty] with an attribute list targeting a property.
        /// </summary>
        public static readonly SuppressionDescriptor FieldAttributeListForObservableProperty = new(
              id: "MVVMTKSPR0002"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Properties using [ObservableProperty] can use [field:] attribute lists to forward attributes to the generated properties"
        );

        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a method using [RelayCommand] with an attribute list targeting a field or property.
        /// </summary>
        public static readonly SuppressionDescriptor FieldOrPropertyAttributeListForRelayCommandMethod = new(
              id: "MVVMTKSPR0003"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Methods using [RelayCommand] can use [field:] and [property:] attribute lists to forward attributes to the generated fields and properties"
        );

        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a method using [BindingProperty] with an attribute list targeting a field or property.
        /// </summary>
        public static readonly SuppressionDescriptor BindingPropertyAttributeListForBindingMethod = new(
              id: "MVVMTKSPR0004"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Methods using [BindingProperty] can use [field:] attribute lists to forward attributes to the generated fields"
        );

        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a method using [BindingCommand] with an attribute list targeting a field or property.
        /// </summary>
        public static readonly SuppressionDescriptor BindingCommandAttributeListForBindingMethod = new(
              id: "MVVMTKSPR0005"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Methods using [BindingCommand] can use [field:] attribute lists to forward attributes to the generated fields"
        );
    }
}
