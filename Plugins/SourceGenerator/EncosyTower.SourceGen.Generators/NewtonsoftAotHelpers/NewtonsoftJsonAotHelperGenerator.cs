using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.NewtonsoftAotHelpers
{
    [Generator]
    internal sealed class NewtonsoftJsonAotHelperGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Serialization.NewtonsoftJson";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(NewtonsoftJsonAotHelperGenerator);
        private const string ATTRIBUTE_METADATA = $"{NAMESPACE}.NewtonsoftJsonAotHelperAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var helperProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ATTRIBUTE_METADATA
                    , ValidateNode
                    , BuildModel
                )
                .Where(static t => t.IsValid);

            var combined = helperProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool ValidateNode(SyntaxNode node, CancellationToken _)
        {
            return node is TypeDeclarationSyntax s
                && s.Kind() is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration;
        }

        private static NewtonsoftAotHelperSpec BuildModel(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax syntax
                || context.TargetSymbol is not INamedTypeSymbol symbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            if (symbol.IsAbstract || symbol.IsUnboundGenericType)
            {
                return default;
            }

            var attrib = context.Attributes[0];
            var args = attrib.ConstructorArguments;

            if (args.Length != 1 || args[0].Value is not INamedTypeSymbol baseType)
            {
                return default;
            }

            var ns = symbol.ContainingNamespace;
            var namespaceName = ns is { IsGlobalNamespace: false }
                ? ns.ToDisplayString()
                : string.Empty;

            using var typeCandidatesBuilder = ImmutableArrayBuilder<AotTypeSpec>.Rent();
            CollectDerivedTypes(
                  context.SemanticModel.Compilation
                , baseType
                , typeCandidatesBuilder
                , token
            );

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  context.TargetNode
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var hintName = syntax.SyntaxTree.GetHintName(context.TargetNode, symbol.ToFileName());

            return new NewtonsoftAotHelperSpec {
                location = LocationInfo.From(context.TargetNode.GetLocation()),
                openingSource = openingSource,
                closingSource = closingSource,
                typeName = symbol.Name,
                hintName = hintName,
                baseTypeFullName = baseType.ToFullName(),
                namespaceName = namespaceName,
                typeCandidates = typeCandidatesBuilder.ToImmutable().AsEquatableArray(),
                containingTypes = symbol.GetContainingTypes(token),
                isStatic = symbol.IsStatic,
                isRecord = symbol.IsRecord,
                typeKind = symbol.TypeKind,
            };
        }

        private static void CollectDerivedTypes(
              Compilation compilation
            , INamedTypeSymbol baseType
            , ImmutableArrayBuilder<AotTypeSpec> builder
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var allTypeSymbols = compilation.GetSymbolsWithName(
                  static _ => true
                , SymbolFilter.Type
                , token
            );

            foreach (var symbol in allTypeSymbols)
            {
                token.ThrowIfCancellationRequested();

                if (symbol is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct))
                {
                    continue;
                }

                if (type.IsAbstract || type.IsUnboundGenericType)
                {
                    continue;
                }

                var equalityComparer = SymbolEqualityComparer.Default;
                var isDerived = equalityComparer.Equals(type, baseType);

                if (isDerived == false)
                {
                    var ancestor = type.BaseType;

                    while (ancestor != null)
                    {
                        token.ThrowIfCancellationRequested();

                        if (equalityComparer.Equals(ancestor, baseType))
                        {
                            isDerived = true;
                            break;
                        }

                        ancestor = ancestor.BaseType;
                    }
                }

                if (isDerived == false)
                {
                    continue;
                }

                using var fieldsBuilder = ImmutableArrayBuilder<AotFieldSpec>.Rent();
                {
                    var typeWalker = type;

                    while (typeWalker != null)
                    {
                        token.ThrowIfCancellationRequested();

                        foreach (var typeMember in typeWalker.GetMembers())
                        {
                            token.ThrowIfCancellationRequested();

                            if (typeMember is not IFieldSymbol field
                                || field.Type is not INamedTypeSymbol fieldType
                            )
                            {
                                continue;
                            }

                            var fieldTypeFullName = fieldType.ToFullName();
                            var fieldTypeCanEnsure = CanEnsureType(fieldType, token);

                            if (fieldType.IsGenericType == false || fieldType.IsUnboundGenericType)
                            {
                                fieldsBuilder.Add(new AotFieldSpec {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.None,
                                });
                                continue;
                            }

                            if (fieldType.TypeArguments.Length == 2
                                && fieldTypeFullName.StartsWith("global::System.Collections.Generic.Dictionary<", StringComparison.Ordinal)
                                && fieldType.TypeArguments[0] is INamedTypeSymbol keyArg
                                && fieldType.TypeArguments[1] is INamedTypeSymbol valueArg
                            )
                            {
                                fieldsBuilder.Add(new AotFieldSpec {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.Dictionary,
                                    elementOrKey = MakeTypeArgInfo(keyArg, token),
                                    dictionaryValue = MakeTypeArgInfo(valueArg, token),
                                });
                                continue;
                            }

                            if (fieldType.TypeArguments.Length == 1
                                && fieldTypeFullName.StartsWith("global::System.Collections.Generic.List<", StringComparison.Ordinal)
                                && fieldType.TypeArguments[0] is INamedTypeSymbol listArg
                            )
                            {
                                fieldsBuilder.Add(new AotFieldSpec {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.List,
                                    elementOrKey = MakeTypeArgInfo(listArg, token),
                                });
                                continue;
                            }

                            if (fieldType.TypeArguments.Length == 1
                                && fieldTypeFullName.StartsWith("global::System.Collections.Generic.HashSet<", StringComparison.Ordinal)
                                && fieldType.TypeArguments[0] is INamedTypeSymbol hashSetArg
                            )
                            {
                                fieldsBuilder.Add(new AotFieldSpec {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.HashSet,
                                    elementOrKey = MakeTypeArgInfo(hashSetArg, token),
                                });
                                continue;
                            }

                            using var otherTypeArgsBuilder = ImmutableArrayBuilder<AotTypeArgSpec>.Rent();

                            foreach (var typeArg in fieldType.TypeArguments)
                            {
                                token.ThrowIfCancellationRequested();

                                if (typeArg is INamedTypeSymbol namedTypeArg)
                                {
                                    otherTypeArgsBuilder.Add(MakeTypeArgInfo(namedTypeArg, token));
                                }
                            }

                            fieldsBuilder.Add(new AotFieldSpec {
                                fieldTypeFullName = fieldTypeFullName,
                                fieldTypeCanEnsure = fieldTypeCanEnsure,
                                collectionKind = AotCollectionKind.None,
                                otherTypeArgs = otherTypeArgsBuilder.ToImmutable().AsEquatableArray(),
                            });
                        }

                        typeWalker = typeWalker.BaseType;
                    }
                }

                builder.Add(new AotTypeSpec {
                    fullName = type.ToFullName(),
                    fields = fieldsBuilder.ToImmutable().AsEquatableArray(),
                });
            }
        }

        private static AotTypeArgSpec MakeTypeArgInfo(INamedTypeSymbol typeArg, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return new AotTypeArgSpec {
                fullName = typeArg.ToFullName(),
                canEnsure = CanEnsureType(typeArg, token),
            };
        }

        private static bool CanEnsureType(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (type.IsAbstract
                || type.SpecialType.IsSystemType()
                || type.TypeKind is not (TypeKind.Struct or TypeKind.Class)
            )
            {
                return false;
            }

            if (type.TypeKind == TypeKind.Class)
            {
                foreach (var ctor in type.Constructors)
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor.Parameters.Length == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , NewtonsoftAotHelperSpec helperInfo
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (compilation.isValid == false
                || helperInfo.IsValid == false
                || helperInfo.typeCandidates.Count < 1
            )
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var assemblyName = compilation.assemblyName;
                var hintName = helperInfo.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , helperInfo.openingSource
                    , HelperDeclaration.WriteCode(helperInfo, helperInfo.typeCandidates.AsImmutableArray())
                    , helperInfo.closingSource
                    , hintName
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
                    , helperInfo.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_NEWTONSOFT_JSON_AOT_HELPER_UNKNOWN_0001"
                , "Newtonsoft Json Aot Helper Generator Error"
                , "This error indicates a bug in the Newtonsoft Json Aot Helper source generators. Error message: '{0}'."
                , NAMESPACE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using NSJU = global::Newtonsoft.Json.Utilities;");
            p.PrintLine("using UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
