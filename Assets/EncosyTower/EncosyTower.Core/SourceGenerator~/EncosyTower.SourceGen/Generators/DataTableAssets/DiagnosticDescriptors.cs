// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MustBeApplicableForTypeArgument = new DiagnosticDescriptor(
              id: "DATA_TABLE_ASSET_0001"
            , title: "Must be either a struct, a class or an enum to replace type argument"
            , messageFormat: "Type \"{0}\" is not applicable to replace \"{1}\", must be either a struct, a class or an enum"
            , category: "DataTableAssetGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Must be either a struct, a class or an enum."
        );
    }
}
