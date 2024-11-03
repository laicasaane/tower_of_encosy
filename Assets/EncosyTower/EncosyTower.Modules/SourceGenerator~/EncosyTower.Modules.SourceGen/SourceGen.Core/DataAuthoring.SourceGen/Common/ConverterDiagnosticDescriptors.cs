// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    internal static class ConverterDiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MissingDefaultConstructor = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0010"
            , title: "Missing default constructor"
            , messageFormat: "The type \"{0}\" must contain a default (parameterless) constructor"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must contain a default (parameterless) constructor to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor StaticConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0020"
            , title: "Static \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public static methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must contain exactly 1 public static method named \"Convert\" to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor InstancedConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0021"
            , title: "Instanced \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public instanced methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must contain exactly 1 public instanced method named \"Convert\" to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor MissingConvertMethod = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0030"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must contain exactly 1 public method named \"Convert\" to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor MissingConvertMethodReturnType = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0031"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must contain exactly 1 public method named \"Convert\" to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethodReturnType = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0040"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The public static \"Convert\" method must conform to the valid format."
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethodReturnType = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0041"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The public static instanced \"Convert\" method must conform to the valid format."
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethod = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0042"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The public \"Convert\" method must conform to the valid format."
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethod = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0043"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The public instanced \"Convert\" method must conform to the valid format."
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0050"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The first argument must be a 'typeof' expression."
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpressionAt = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0051"
            , title: "Not a typeof expression"
            , messageFormat: "The argument at position {0} must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The argument must be a 'typeof' expression."
        );

        public static readonly DiagnosticDescriptor AbstractTypeNotSupported = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0060"
            , title: "Abstract type is not supported"
            , messageFormat: "The type \"{0}\" must not be abstract"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be abstract to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0061"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be open generic to be considered a valid converter."
        );

        public static readonly DiagnosticDescriptor ConverterAmbiguity = new DiagnosticDescriptor(
              id: "DATABASE_CONVERTER_0070"
            , title: "Converter ambiguity"
            , messageFormat: "The type \"{0}\" at position {3} will be ignored because a \"Convert\" method that returns a value of \"{2}\" has already been defined in \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Multiple converters whose \"Convert\" method returns a value of the same type are not allowed."
        );
    }
}
