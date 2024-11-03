// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Data.SourceGen
{
    /// <summary>
    /// A container for all <see cref="SuppressionDescriptors"/> instances for suppressed diagnostics by analyzers in this project.
    /// </summary>
    internal static class SuppressionDescriptors
    {
        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a field using [SerializeField] with an attribute list targeting a property.
        /// </summary>
        public static readonly SuppressionDescriptor PropertyAttributeListForDataField = new(
              id: "DATASPR0001"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Fields using [SerializeField] can use [property:] attribute lists to forward attributes to the generated properties"
        );

        /// <summary>
        /// Gets a <see cref="SuppressionDescriptor"/> for a property using [DataProperty] with an attribute list targeting a field.
        /// </summary>
        public static readonly SuppressionDescriptor FieldAttributeListForDataProperty = new(
              id: "DATASPR0002"
            , suppressedDiagnosticId: "CS0657"
            , justification: "Properties using [DataProperty] can use [field:] attribute lists to forward attributes to the generated fields"
        );
    }
}
