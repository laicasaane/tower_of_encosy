using System;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    [Generator]
    public sealed class TypeWrapGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.TypeWraps";
        public const string WRAP_TYPE = "WrapType";
        public const string WRAP_TYPE_ATTRIBUTE = $"global::{NAMESPACE}.WrapTypeAttribute";
        public const string WRAP_RECORD = "WrapRecord";
        public const string WRAP_RECORD_ATTRIBUTE = $"global::{NAMESPACE}.WrapRecordAttribute";
        public const string GENERATOR_NAME = nameof(TypeWrapGenerator);
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string WRAP_TYPE_ATTRIBUTE_METADATA = $"{NAMESPACE}.WrapTypeAttribute";
        private const string WRAP_RECORD_ATTRIBUTE_METADATA = $"{NAMESPACE}.WrapRecordAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var wrapTypeProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  WRAP_TYPE_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax or ClassDeclarationSyntax
                , ExtractSpecForWrapType
            ).Where(static t => t.IsValid);

            var wrapRecordProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  WRAP_RECORD_ATTRIBUTE_METADATA
                , static (node, _) => node is RecordDeclarationSyntax recordSyntax
                    && recordSyntax.ParameterList != null
                    && recordSyntax.ParameterList.Parameters.Count > 0
                , ExtractSpecForWrapRecord
            ).Where(static t => t.IsValid);

            var combinedWrapType = wrapTypeProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            var combinedWrapRecord = wrapRecordProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combinedWrapType, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });

            context.RegisterSourceOutput(combinedWrapRecord, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        public static TypeWrapSpec ExtractSpecForWrapType(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var enableNullable = semanticModel.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            switch (context.TargetNode)
            {
                case StructDeclarationSyntax structSyntax:
                {
                    if (TryGetWrapTypeInfo(structSyntax, token, out var candidate))
                    {
                        GetTypeName(structSyntax, token, ref candidate);
                        SetOtherFields(ref candidate, structSyntax, symbol, semanticModel, token);
                        candidate.isStruct = true;
                        candidate.isRefStruct = structSyntax.Modifiers.Any(SyntaxKind.RefKeyword);

                        var syntaxTree = structSyntax.SyntaxTree;
                        var fileTypeName = symbol.ToFileName();
                        var hintName = syntaxTree.GetHintName(structSyntax, fileTypeName);

                        TypeCreationHelpers.GenerateOpeningAndClosingSource(
                              structSyntax
                            , token
                            , out var openingSource
                            , out var closingSource
                            , printAdditionalUsings: PrintAdditionalUsings
                        );

                        return new TypeWrapSpec(
                              LocationInfo.From(candidate.syntax.GetLocation())
                            , hintName
                            , openingSource
                            , closingSource
                            , candidate.symbol
                            , candidate.typeName
                            , candidate.typeNameWithTypeParams
                            , candidate.isStruct
                            , candidate.isRefStruct
                            , candidate.isRecord
                            , candidate.fieldTypeSymbol
                            , candidate.fieldName
                            , candidate.excludeConverter || candidate.isGeneric
                            , enableNullable
                        );
                    }

                    break;
                }

                case ClassDeclarationSyntax classSyntax:
                {
                    if (TryGetWrapTypeInfo(classSyntax, token, out var candidate)
                        && InheritBaseClass(symbol, token) == false
                    )
                    {
                        GetTypeName(classSyntax, token, ref candidate);
                        SetOtherFields(ref candidate, classSyntax, symbol, semanticModel, token);

                        var syntaxTree = classSyntax.SyntaxTree;
                        var fileTypeName = symbol.ToFileName();
                        var hintName = syntaxTree.GetHintName(classSyntax, fileTypeName);

                        TypeCreationHelpers.GenerateOpeningAndClosingSource(
                              classSyntax
                            , token
                            , out var openingSource
                            , out var closingSource
                            , printAdditionalUsings: PrintAdditionalUsings
                        );

                        return new TypeWrapSpec(
                              LocationInfo.From(candidate.syntax.GetLocation())
                            , hintName
                            , openingSource
                            , closingSource
                            , candidate.symbol
                            , candidate.typeName
                            , candidate.typeNameWithTypeParams
                            , candidate.isStruct
                            , candidate.isRefStruct
                            , candidate.isRecord
                            , candidate.fieldTypeSymbol
                            , candidate.fieldName
                            , candidate.excludeConverter || candidate.isGeneric
                            , enableNullable
                        );
                    }

                    break;
                }
            }

            return default;
        }

        public static TypeWrapSpec ExtractSpecForWrapRecord(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not RecordDeclarationSyntax recordSyntax
                || context.TargetSymbol is not INamedTypeSymbol symbol
            )
            {
                return default;
            }

            if (recordSyntax.ParameterList is not ParameterListSyntax { Parameters.Count: 1 })
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var enableNullable = semanticModel.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            if (TryGetWrapRecordInfo(recordSyntax, token, out var candidate)
                && (recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) == false
                    || InheritBaseClass(symbol, token) == false)
            )
            {
                GetTypeName(recordSyntax, token, ref candidate);
                SetOtherFields(ref candidate, recordSyntax, symbol, semanticModel, token);
                candidate.isStruct = recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);
                candidate.isRecord = true;

                var syntaxTree = recordSyntax.SyntaxTree;
                var fileTypeName = symbol.ToFileName();
                var hintName = syntaxTree.GetHintName(recordSyntax, fileTypeName);

                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      recordSyntax
                    , token
                    , out var openingSource
                    , out var closingSource
                    , printAdditionalUsings: PrintAdditionalUsings
                );

                return new TypeWrapSpec(
                      LocationInfo.From(candidate.syntax.GetLocation())
                    , hintName
                    , openingSource
                    , closingSource
                    , candidate.symbol
                    , candidate.typeName
                    , candidate.typeNameWithTypeParams
                    , candidate.isStruct
                    , candidate.isRefStruct
                    , candidate.isRecord
                    , candidate.fieldTypeSymbol
                    , candidate.fieldName
                    , candidate.excludeConverter || candidate.isGeneric
                    , enableNullable
                );
            }

            return default;
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SCM = System.ComponentModel;");
            p.PrintLine("using SC = global::System.Collections;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SD = global::System.Diagnostics;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SG = global::System.Globalization;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETTW = global::EncosyTower.TypeWraps;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static bool InheritBaseClass(INamedTypeSymbol classSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var baseType = classSymbol.BaseType;

            while (baseType != null)
            {
                token.ThrowIfCancellationRequested();

                if (baseType.TypeKind == TypeKind.Class
                    && baseType.SpecialType != SpecialType.System_Object
                )
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private static bool TryGetWrapTypeInfo(
              TypeDeclarationSyntax syntax
            , CancellationToken token
            , out Candidate result
        )
        {
            token.ThrowIfCancellationRequested();

            result = new Candidate {
                fieldName = string.Empty,
            };

            TypeSyntax fieldTypeSyntax = null;

            foreach (var attribList in syntax.AttributeLists)
            {
                token.ThrowIfCancellationRequested();

                foreach (var attrib in attribList.Attributes)
                {
                    token.ThrowIfCancellationRequested();

                    var argumentList = attrib.ArgumentList;

                    if (argumentList == null)
                    {
                        continue;
                    }

                    var arguments = argumentList.Arguments;

                    if (arguments.Count < 1)
                    {
                        continue;
                    }

                    if (attrib.Name.IsTypeNameCandidate(NAMESPACE, WRAP_TYPE, token) == false)
                    {
                        continue;
                    }

                    foreach (var arg in arguments)
                    {
                        token.ThrowIfCancellationRequested();

                        if (arg.NameEquals != null)
                        {
                            switch (arg.NameEquals.Name.Identifier.Text)
                            {
                                case "ExcludeConverter":
                                {
                                    if (arg.Expression is LiteralExpressionSyntax literal)
                                    {
                                        result.excludeConverter = (bool)literal.Token.Value;
                                    }

                                    break;
                                }
                            }
                        }
                        else
                        {
                            switch (arg.Expression)
                            {
                                case TypeOfExpressionSyntax typeOf:
                                {
                                    fieldTypeSyntax = typeOf.Type;
                                    break;
                                }

                                case LiteralExpressionSyntax literal:
                                {
                                    result.fieldName = literal.Token.ValueText;
                                    break;
                                }

                                case InvocationExpressionSyntax invocation:
                                {
                                    if (invocation.Expression is IdentifierNameSyntax identifierName
                                        && identifierName.Identifier.ValueText == "nameof"
                                        && invocation.ArgumentList.Arguments.Count == 1
                                    )
                                    {
                                        var nameofArg = invocation.ArgumentList.Arguments[0];

                                        if (nameofArg.Expression is IdentifierNameSyntax idName)
                                        {
                                            result.fieldName = idName.Identifier.ValueText;
                                        }
                                        else if (nameofArg.Expression is MemberAccessExpressionSyntax memberAccess)
                                        {
                                            result.fieldName = memberAccess.Name.Identifier.ValueText;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (fieldTypeSyntax != null)
                    {
                        break;
                    }
                }

                if (fieldTypeSyntax != null)
                {
                    break;
                }
            }

            if (fieldTypeSyntax != null)
            {
                result.fieldTypeSyntax = fieldTypeSyntax;
            }

            return result.fieldTypeSyntax != null;
        }

        private static bool TryGetWrapRecordInfo(
              RecordDeclarationSyntax syntax
            , CancellationToken token
            , out Candidate result
        )
        {
            token.ThrowIfCancellationRequested();

            result = new Candidate();

            foreach (var attribList in syntax.AttributeLists)
            {
                token.ThrowIfCancellationRequested();

                var alreadProcessed = false;

                foreach (var attrib in attribList.Attributes)
                {
                    token.ThrowIfCancellationRequested();

                    if (attrib.Name.IsTypeNameCandidate(NAMESPACE, WRAP_RECORD, token) == false)
                    {
                        continue;
                    }

                    var argumentList = attrib.ArgumentList;

                    if (argumentList != null)
                    {
                        var arguments = argumentList.Arguments;

                        foreach (var arg in arguments)
                        {
                            token.ThrowIfCancellationRequested();

                            if (arg.NameEquals == null)
                            {
                                continue;
                            }

                            switch (arg.NameEquals.Name.Identifier.Text)
                            {
                                case "ExcludeConverter":
                                {
                                    if (arg.Expression is LiteralExpressionSyntax literal)
                                    {
                                        result.excludeConverter = (bool)literal.Token.Value;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    alreadProcessed = true;
                    break;
                }

                if (alreadProcessed)
                {
                    break;
                }
            }

            if (syntax.ParameterList != null
                && syntax.ParameterList.Parameters.Count > 0
                && syntax.ParameterList.Parameters[0].Type is TypeSyntax targetTypeSyntax
            )
            {
                result.fieldTypeSyntax = targetTypeSyntax;
                result.fieldName = syntax.ParameterList.Parameters[0].Identifier.ValueText;
            }

            return result.fieldTypeSyntax != null;
        }

        private static void GetTypeName(
              TypeDeclarationSyntax syntax
            , CancellationToken token
            , ref Candidate candidate
        )
        {
            token.ThrowIfCancellationRequested();

            var typeNameWithTypeParamsBuilder = new StringBuilder(syntax.Identifier.ValueText);

            if (syntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                candidate.isGeneric = true;

                typeNameWithTypeParamsBuilder.Append("<");

                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    token.ThrowIfCancellationRequested();

                    typeNameWithTypeParamsBuilder.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        typeNameWithTypeParamsBuilder.Append(", ");
                    }
                }

                typeNameWithTypeParamsBuilder.Append(">");
            }

            candidate.typeName = syntax.Identifier.ValueText;
            candidate.typeNameWithTypeParams = typeNameWithTypeParamsBuilder.ToString();
        }

        private static void SetOtherFields(
              ref Candidate candidate
            , TypeDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            candidate.fieldTypeSymbol = semanticModel.GetSymbolInfo(candidate.fieldTypeSyntax, token).Symbol as INamedTypeSymbol;
            candidate.syntax = syntax;
            candidate.symbol = symbol;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , TypeWrapSpec declaration
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (declaration.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = declaration.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.openingSource
                    , declaration.WriteCode()
                    , declaration.closingSource
                    , declaration.hintName
                    , sourceFilePath
                    , projectPath
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , declaration.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_TYPE_WRAP_UNKNOWN_0001"
                , "Type Wrap Generator Error"
                , "This error indicates a bug in the Type Wrap source generators. Error message: '{0}'."
                , "TypeWrap"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        public partial struct Candidate : IEquatable<Candidate>
        {
            public TypeDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;
            public string typeName;
            public string typeNameWithTypeParams;
            public bool isGeneric;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;
            public TypeSyntax fieldTypeSyntax;
            public INamedTypeSymbol fieldTypeSymbol;
            public string fieldName;
            public bool excludeConverter;

            public readonly bool IsValid
                => syntax is { }
                && symbol is { }
                && fieldTypeSyntax is { }
                && fieldTypeSymbol is { }
                && string.IsNullOrEmpty(typeName) == false
                && string.IsNullOrEmpty(typeNameWithTypeParams) == false
                && fieldTypeSymbol.TypeKind != TypeKind.Dynamic;

            public readonly override bool Equals(object obj)
                => obj is Candidate other && Equals(other);

            public readonly bool Equals(Candidate other)
                => string.Equals(typeNameWithTypeParams, other.typeNameWithTypeParams, StringComparison.Ordinal)
                && string.Equals(fieldTypeSymbol?.ToFullName() ?? string.Empty, other.fieldTypeSymbol?.ToFullName() ?? string.Empty)
                && fieldTypeSymbol?.TypeKind == other.fieldTypeSymbol?.TypeKind
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      typeNameWithTypeParams
                    , fieldTypeSymbol?.ToFullName() ?? string.Empty
                    , fieldTypeSymbol?.TypeKind
                );
        }
    }
}
