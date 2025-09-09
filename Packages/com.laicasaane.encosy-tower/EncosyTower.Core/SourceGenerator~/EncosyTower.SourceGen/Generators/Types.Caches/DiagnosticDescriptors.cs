// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.Types.Caches
{
    /// <summary>
    /// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
    /// </summary>
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor TypeParameterIsNotApplicable = new DiagnosticDescriptor(
              id: "RUNTIME_TYPE_CACHES_0001"
            , title: "Type parameter is not applicable"
            , messageFormat: "\"{0}\" is a type parameter thus it is not applicable for the \"{1}\" method"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Type parameter is not applicable."
        );

        public static readonly DiagnosticDescriptor OnlyClassOrInterfaceIsApplicable = new DiagnosticDescriptor(
              id: "RUNTIME_TYPE_CACHES_0002"
            , title: "Only class or interface is applicable"
            , messageFormat: "\"{0}\" is not applicable because it is not class nor interface"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Only class or interface is applicable."
        );

        public static readonly DiagnosticDescriptor StaticClassIsNotApplicable = new DiagnosticDescriptor(
              id: "RUNTIME_TYPE_CACHES_0003"
            , title: "Static class is not applicable"
            , messageFormat: "\"{0}\" is not applicable because it is static"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Static class is not applicable."
        );

        public static readonly DiagnosticDescriptor SealedClassIsNotApplicable = new DiagnosticDescriptor(
              id: "RUNTIME_TYPE_CACHES_0004"
            , title: "Sealed class is not applicable"
            , messageFormat: "\"{0}\" is not applicable because it is sealed"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Sealed class is not applicable."
        );

        public static readonly DiagnosticDescriptor AssemblyNameMustBeStringLiteralOrConstant = new DiagnosticDescriptor(
              id: "RUNTIME_TYPE_CACHES_0005"
            , title: "Assembly name must be a string literal or constant"
            , messageFormat: "Assembly name must be a string literal or constant"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Assembly name must be a string literal or constant."
        );

        public static readonly DiagnosticDescriptor TypesFromCachesAreProhibited = new DiagnosticDescriptor(
              id: "RUNTIME_TYPE_CACHES_0006"
            , title: "Types from \"EncosyTower.Types.Caches\" are prohibited"
            , messageFormat: "\"{0}\" is prohibited"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Types from \"EncosyTower.Types.Caches\" are prohibited."
        );
    }
}
