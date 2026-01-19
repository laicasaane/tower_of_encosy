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
    public class TypeWrapGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.TypeWraps";
        public const string WRAP_TYPE = "WrapType";
        public const string WRAP_TYPE_ATTRIBUTE = $"global::{NAMESPACE}.WrapTypeAttribute";
        public const string WRAP_RECORD = "WrapRecord";
        public const string WRAP_RECORD_ATTRIBUTE = $"global::{NAMESPACE}.WrapRecordAttribute";
        public const string GENERATOR_NAME = nameof(TypeWrapGenerator);
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidTypeSyntax,
                transform: GetSemanticSymbolMatch
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsValidTypeSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (node is not TypeDeclarationSyntax type)
            {
                return false;
            }

            if (type.AttributeLists.Count < 1)
            {
                return false;
            }

            if (type is RecordDeclarationSyntax recordSyntax)
            {
                return type.HasAttributeCandidate(NAMESPACE, WRAP_RECORD)
                    && recordSyntax.ParameterList != null
                    && recordSyntax.ParameterList.Parameters.Count > 0;
            }

            if (type is StructDeclarationSyntax or ClassDeclarationSyntax)
            {
                return type.HasAttributeCandidate(NAMESPACE, WRAP_TYPE);
            }

            return false;
        }

        public static TypeWrapDeclaration GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var semanticModel = context.SemanticModel;
            var enableNullable = semanticModel.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            switch (context.Node)
            {
                case RecordDeclarationSyntax recordSyntax:
                {
                    if (recordSyntax.ParameterList is ParameterListSyntax { Parameters.Count: 1 }
                        && TryGetWrapRecordInfo(recordSyntax, out var candidate)
                        && semanticModel.GetDeclaredSymbol(recordSyntax, token) is INamedTypeSymbol symbol
                        && symbol.HasAttribute(WRAP_RECORD_ATTRIBUTE)
                    )
                    {
                        if (recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) == false
                            || InheritBaseClass(semanticModel, symbol, token) == false
                        )
                        {
                            GetTypeName(recordSyntax, ref candidate);
                            SetOtherFields(ref candidate, recordSyntax, symbol, semanticModel, token);
                            candidate.isStruct = recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);
                            candidate.isRecord = true;

                            var syntaxTree = recordSyntax.SyntaxTree;
                            var fileTypeName = symbol.ToFileName();
                            var hintName = syntaxTree.GetGeneratedSourceFileName(
                                  GENERATOR_NAME
                                , recordSyntax
                                , fileTypeName
                            );

                            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                                  semanticModel.Compilation.AssemblyName
                                , GENERATOR_NAME
                                , fileTypeName
                            );

                            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                                  recordSyntax
                                , token
                                , out var openingSource
                                , out var closingSource
                                , printAdditionalUsings: PrintAdditionalUsings
                            );

                            return new TypeWrapDeclaration(
                                  candidate.syntax.GetLocation()
                                , hintName
                                , sourceFilePath
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
                    }

                    break;
                }

                case StructDeclarationSyntax structSyntax:
                {
                    if (TryGetWrapTypeInfo(structSyntax, out var candidate)
                        && semanticModel.GetDeclaredSymbol(structSyntax, token) is INamedTypeSymbol symbol
                        && symbol.HasAttribute(WRAP_TYPE_ATTRIBUTE)
                    )
                    {
                        GetTypeName(structSyntax, ref candidate);
                        SetOtherFields(ref candidate, structSyntax, symbol, semanticModel, token);
                        candidate.isStruct = true;
                        candidate.isRefStruct = structSyntax.Modifiers.Any(SyntaxKind.RefKeyword);

                        var syntaxTree = structSyntax.SyntaxTree;
                        var fileTypeName = symbol.ToFileName();
                        var hintName = syntaxTree.GetGeneratedSourceFileName(
                                  GENERATOR_NAME
                                , structSyntax
                                , fileTypeName
                            );

                        var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                                  semanticModel.Compilation.AssemblyName
                                , GENERATOR_NAME
                                , fileTypeName
                            );

                        TypeCreationHelpers.GenerateOpeningAndClosingSource(
                              structSyntax
                            , token
                            , out var openingSource
                            , out var closingSource
                            , printAdditionalUsings: PrintAdditionalUsings
                        );

                        return new TypeWrapDeclaration(
                              candidate.syntax.GetLocation()
                            , hintName
                            , sourceFilePath
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
                    if (TryGetWrapTypeInfo(classSyntax, out var candidate)
                        && semanticModel.GetDeclaredSymbol(classSyntax, token) is INamedTypeSymbol symbol
                        && symbol.HasAttribute(WRAP_TYPE_ATTRIBUTE)
                    )
                    {
                        if (InheritBaseClass(semanticModel, symbol, token) == false)
                        {
                            GetTypeName(classSyntax, ref candidate);
                            SetOtherFields(ref candidate, classSyntax, symbol, semanticModel, token);

                            var syntaxTree = classSyntax.SyntaxTree;
                            var fileTypeName = symbol.ToFileName();
                            var hintName = syntaxTree.GetGeneratedSourceFileName(
                                  GENERATOR_NAME
                                , classSyntax
                                , fileTypeName
                            );

                            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                                  semanticModel.Compilation.AssemblyName
                                , GENERATOR_NAME
                                , fileTypeName
                            );

                            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                                  classSyntax
                                , token
                                , out var openingSource
                                , out var closingSource
                                , printAdditionalUsings: PrintAdditionalUsings
                            );

                            return new TypeWrapDeclaration(
                                  candidate.syntax.GetLocation()
                                , hintName
                                , sourceFilePath
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
                    }

                    break;
                }

                static void PrintAdditionalUsings(ref Printer p)
                {
                    p.PrintEndLine();
                    p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                    p.PrintEndLine();
                    p.PrintLine("using System;");
                    p.PrintLine("using System.ComponentModel;");
                    p.PrintLine("using System.CodeDom.Compiler;");
                    p.PrintLine("using System.Diagnostics;");
                    p.PrintLine("using System.Diagnostics.CodeAnalysis;");
                    p.PrintLine("using System.Globalization;");
                    p.PrintLine("using System.Runtime.CompilerServices;");
                    p.PrintLine("using System.Runtime.InteropServices;");
                    p.PrintLine("using EncosyTower.Common;");
                    p.PrintLine("using EncosyTower.TypeWraps;");
                    p.PrintEndLine();
                    p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                    p.PrintEndLine();
                }
            }

            return default;

            static bool InheritBaseClass(
                  SemanticModel semanticModel
                , INamedTypeSymbol classSymbol
                , CancellationToken token
            )
            {
                foreach (var syntax in classSymbol.DeclaringSyntaxReferences)
                {
                    if (semanticModel.GetDeclaredSymbol(syntax.GetSyntax(token), token) is INamedTypeSymbol symbol
                        && symbol.BaseType is INamedTypeSymbol baseType
                        && baseType.TypeKind == TypeKind.Class
                        && baseType.SpecialType != SpecialType.System_Object
                    )
                    {
                        return true;
                    }
                }

                return false;
            }

            static bool TryGetWrapTypeInfo(TypeDeclarationSyntax syntax, out Candidate result)
            {
                result = new Candidate {
                    fieldName = string.Empty,
                };

                TypeSyntax fieldTypeSyntax = null;

                foreach (var attribList in syntax.AttributeLists)
                {
                    foreach (var attrib in attribList.Attributes)
                    {
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

                        if (attrib.Name.IsTypeNameCandidate(NAMESPACE, WRAP_TYPE) == false)
                        {
                            continue;
                        }

                        foreach (var arg in arguments)
                        {
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

            static bool TryGetWrapRecordInfo(RecordDeclarationSyntax syntax, out Candidate result)
            {
                result = new Candidate();

                foreach (var attribList in syntax.AttributeLists)
                {
                    var alreadProcessed = false;

                    foreach (var attrib in attribList.Attributes)
                    {
                        if (attrib.Name.IsTypeNameCandidate(NAMESPACE, WRAP_RECORD) == false)
                        {
                            continue;
                        }

                        var argumentList = attrib.ArgumentList;

                        if (argumentList != null)
                        {
                            var arguments = argumentList.Arguments;

                            foreach (var arg in arguments)
                            {
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

            static void GetTypeName(TypeDeclarationSyntax syntax, ref Candidate candidate)
            {
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

            static void SetOtherFields(
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
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
            , TypeWrapDeclaration declaration
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
                SourceGenHelpers.ProjectPath = projectPath;

                context.OutputSource(
                      outputSourceGenFiles
                    , declaration.OpeningSource
                    , declaration.WriteCode()
                    , declaration.ClosingSource
                    , declaration.HintName
                    , declaration.SourceFilePath
                    , declaration.Location
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
                    , declaration.Location
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_TYPE_WRAP_01"
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
