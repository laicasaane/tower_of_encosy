// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    internal static class TableDiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor NotTypeOfExpression = new DiagnosticDescriptor(
              id: "DATABASE_TABLE_0001"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The first argument must be a 'typeof' expression."
        );

        public static readonly DiagnosticDescriptor AbstractTypeNotSupported = new DiagnosticDescriptor(
              id: "DATABASE_TABLE_0003"
            , title: "Abstract type is not supported"
            , messageFormat: "The type \"{0}\" must not be abstract"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be abstract to be considered a valid table."
        );

        public static readonly DiagnosticDescriptor GenericTypeNotSupported = new DiagnosticDescriptor(
              id: "DATABASE_TABLE_0004"
            , title: "Generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be generic"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be generic to be considered a valid table."
        );

        public static readonly DiagnosticDescriptor MustBeDerivedFromDataTableAsset = new DiagnosticDescriptor(
              id: "DATABASE_TABLE_0005"
            , title: "Type must be derived from either DataTableAsset<TDataId, TData> or DataTableAsset<TDataId, TData, TConvertedId>"
            , messageFormat: "The type \"{0}\" must be derived from either DataTableAsset<TDataId, TData> or DataTableAsset<TDataId, TData, TConvertedId>"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must be derived from either DataTableAsset<TDataId, TData> or DataTableAsset<TDataId, TData, TConvertedId>."
        );
    }
}