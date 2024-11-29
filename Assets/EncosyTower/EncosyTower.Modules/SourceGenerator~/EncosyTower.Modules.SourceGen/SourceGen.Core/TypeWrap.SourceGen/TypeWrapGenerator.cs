using System;
using System.Linq;
using System.Text;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.TypeWrap.SourceGen
{
    [Generator]
    public class TypeWrapGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Modules.TypeWrap";
        public const string WRAP_TYPE = "WrapType";
        public const string WRAP_TYPE_ATTRIBUTE = "global::EncosyTower.Modules.TypeWrap.WrapTypeAttribute";
        public const string WRAP_RECORD = "WrapRecord";
        public const string WRAP_RECORD_ATTRIBUTE = "global::EncosyTower.Modules.TypeWrap.WrapRecordAttribute";
        public const string GENERATOR_NAME = nameof(TypeWrapGenerator);
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.TypeWrap.SkipSourceGenForAssemblyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidTypeSyntax,
                transform: GetSemanticSymbolMatch
            ).Where(t => t is { syntax: { }, symbol: { }, fieldTypeSymbol: { } });

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(SKIP_ATTRIBUTE));

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

        public static Candidate GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var semanticModel = context.SemanticModel;

            switch (context.Node)
            {
                case RecordDeclarationSyntax recordSyntax:
                {
                    if (recordSyntax.ParameterList is ParameterListSyntax { Parameters: { Count: 1 } }
                        && TryGetWrapRecordInfo(recordSyntax, out var candidate)
                        && semanticModel.GetDeclaredSymbol(recordSyntax, token) is INamedTypeSymbol symbol
                        && symbol.HasAttribute(WRAP_RECORD_ATTRIBUTE)
                    )
                    {
                        if (recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) == false
                            || InheritBaseClass(semanticModel, symbol, token) == false
                        )
                        {
                            GetTypeName(recordSyntax, candidate);
                            SetOtherFields(candidate, recordSyntax, symbol, semanticModel, token);
                            candidate.isStruct = recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);
                            candidate.isRecord = true;
                            return candidate;
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
                        GetTypeName(structSyntax, candidate);
                        SetOtherFields(candidate, structSyntax, symbol, semanticModel, token);
                        candidate.isStruct = true;
                        candidate.isRefStruct = structSyntax.Modifiers.Any(SyntaxKind.RefKeyword);
                        return candidate;
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
                            GetTypeName(classSyntax, candidate);
                            SetOtherFields(candidate, classSyntax, symbol, semanticModel, token);
                            return candidate;
                        }
                    }

                    break;
                }
            }

            return null;

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
                    fieldName = "value",
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

            static void GetTypeName(TypeDeclarationSyntax syntax, Candidate candidate)
            {
                var typeNameSb = new StringBuilder(syntax.Identifier.ValueText);

                if (syntax.TypeParameterList is TypeParameterListSyntax typeParamList
                    && typeParamList.Parameters.Count > 0
                )
                {
                    candidate.isGeneric = true;

                    typeNameSb.Append("<");

                    var typeParams = typeParamList.Parameters;
                    var last = typeParams.Count - 1;

                    for (var i = 0; i <= last; i++)
                    {
                        typeNameSb.Append(typeParams[i].Identifier.Text);

                        if (i < last)
                        {
                            typeNameSb.Append(", ");
                        }
                    }

                    typeNameSb.Append(">");
                }

                candidate.typeName = typeNameSb.ToString();
            }

            static void SetOtherFields(
                  Candidate candidate
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
            , CompilationCandidate compilationCandidate
            , Candidate candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate == null
                || candidate.syntax == null
                || candidate.symbol == null
                || candidate.fieldTypeSymbol == null
                || candidate.fieldTypeSymbol.TypeKind == TypeKind.Dynamic
            )
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = candidate.syntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var declaration = new TypeWrapDeclaration(
                      candidate.syntax
                    , candidate.symbol
                    , candidate.typeName
                    , candidate.isStruct
                    , candidate.isRefStruct
                    , candidate.isRecord
                    , candidate.fieldTypeSymbol
                    , candidate.fieldName
                    , candidate.excludeConverter || candidate.isGeneric
                    , compilationCandidate.enableNullable
                );

                var source = declaration.WriteCode();
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);
                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , candidate.syntax
                    , source
                    , context.CancellationToken
                );

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, declaration.Syntax, declaration.Symbol.ToValidIdentifier())
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , candidate.syntax.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , candidate.syntax.GetLocation()
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

        public class Candidate
        {
            public TypeDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;
            public string typeName;
            public bool isGeneric;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;
            public TypeSyntax fieldTypeSyntax;
            public INamedTypeSymbol fieldTypeSymbol;
            public string fieldName;
            public bool excludeConverter;
        }
    }
}
