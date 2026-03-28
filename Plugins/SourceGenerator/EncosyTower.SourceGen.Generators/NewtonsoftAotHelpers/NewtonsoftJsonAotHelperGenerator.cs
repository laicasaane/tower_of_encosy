using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Generators.NewtonsoftJsonHelpers
{
    [Generator]
    internal class NewtonsoftJsonAotHelperGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Serialization.NewtonsoftJson";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(NewtonsoftJsonAotHelperGenerator);

        /// <summary>
        /// CLR metadata name (no <c>global::</c> prefix) required by
        /// <see cref="SyntaxValueProvider.ForAttributeWithMetadataName"/>.
        /// </summary>
        private const string ATTRIBUTE_METADATA = $"{NAMESPACE}.NewtonsoftJsonAotHelperAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationInfo.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var helperProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ATTRIBUTE_METADATA
                    , static (node, _) => node is TypeDeclarationSyntax s
                        && s.Kind() is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration
                    , GetHelperInfo
                )
                .Where(static t => t.IsValid);

            var combined = helperProvider
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

        private static NewtonsoftAotHelperInfo GetHelperInfo(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
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

            using var typeCandidatesBuilder = ImmutableArrayBuilder<AotTypeCandidate>.Rent();
            CollectDerivedTypes(
                  context.SemanticModel.Compilation
                , baseType
                , typeCandidatesBuilder
                , token
            );

            return new NewtonsoftAotHelperInfo {
                location = LocationInfo.From(context.TargetNode.GetLocation()),
                typeName = symbol.Name,
                fileHintName = symbol.ToFileName(),
                baseTypeFullName = baseType.ToFullName(),
                namespaceName = namespaceName,
                typeCandidates = typeCandidatesBuilder.ToImmutable().AsEquatableArray(),
                containingTypes = symbol.GetContainingTypes(),
                isStatic = symbol.IsStatic,
                isRecord = symbol.IsRecord,
                typeKind = symbol.TypeKind,
            };
        }

        private static void CollectDerivedTypes(
              Compilation compilation
            , INamedTypeSymbol baseType
            , ImmutableArrayBuilder<AotTypeCandidate> builder
            , CancellationToken token
        )
        {
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

                // Check whether type IS baseType or baseType appears in the ancestor chain.
                var equalityComparer = SymbolEqualityComparer.Default;
                var isDerived = equalityComparer.Equals(type, baseType);

                if (isDerived == false)
                {
                    var ancestor = type.BaseType;

                    while (ancestor != null)
                    {
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

                // Flatten all fields from the full type hierarchy into one list.
                using var fieldsBuilder = ImmutableArrayBuilder<AotFieldInfo>.Rent();
                {
                    var typeWalker = type;

                    while (typeWalker != null)
                    {
                        token.ThrowIfCancellationRequested();

                        foreach (var typeMember in typeWalker.GetMembers())
                        {
                            if (typeMember is not IFieldSymbol field
                                || field.Type is not INamedTypeSymbol fieldType
                            )
                            {
                                continue;
                            }

                            var fieldTypeFullName = fieldType.ToFullName();
                            var fieldTypeCanEnsure = CanEnsureType(fieldType);

                            // Non-generic or unbound-generic: only the field type itself matters.
                            if (fieldType.IsGenericType == false || fieldType.IsUnboundGenericType)
                            {
                                fieldsBuilder.Add(new AotFieldInfo {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.None,
                                });
                                continue;
                            }

                            // Dictionary<K, V>
                            if (fieldType.TypeArguments.Length == 2
                                && fieldTypeFullName.StartsWith("global::System.Collections.Generic.Dictionary<", StringComparison.Ordinal)
                                && fieldType.TypeArguments[0] is INamedTypeSymbol keyArg
                                && fieldType.TypeArguments[1] is INamedTypeSymbol valueArg
                            )
                            {
                                fieldsBuilder.Add(new AotFieldInfo {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.Dictionary,
                                    elementOrKey = MakeTypeArgInfo(keyArg),
                                    dictionaryValue = MakeTypeArgInfo(valueArg),
                                });
                                continue;
                            }

                            // List<T>
                            if (fieldType.TypeArguments.Length == 1
                                && fieldTypeFullName.StartsWith("global::System.Collections.Generic.List<", StringComparison.Ordinal)
                                && fieldType.TypeArguments[0] is INamedTypeSymbol listArg
                            )
                            {
                                fieldsBuilder.Add(new AotFieldInfo {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.List,
                                    elementOrKey = MakeTypeArgInfo(listArg),
                                });
                                continue;
                            }

                            // HashSet<T>
                            if (fieldType.TypeArguments.Length == 1
                                && fieldTypeFullName.StartsWith("global::System.Collections.Generic.HashSet<", StringComparison.Ordinal)
                                && fieldType.TypeArguments[0] is INamedTypeSymbol hashSetArg
                            )
                            {
                                fieldsBuilder.Add(new AotFieldInfo {
                                    fieldTypeFullName = fieldTypeFullName,
                                    fieldTypeCanEnsure = fieldTypeCanEnsure,
                                    collectionKind = AotCollectionKind.HashSet,
                                    elementOrKey = MakeTypeArgInfo(hashSetArg),
                                });
                                continue;
                            }

                            // Other generic: collect named type arguments.
                            using var otherTypeArgsBuilder = ImmutableArrayBuilder<AotTypeArgInfo>.Rent();

                            foreach (var typeArg in fieldType.TypeArguments)
                            {
                                if (typeArg is INamedTypeSymbol namedTypeArg)
                                {
                                    otherTypeArgsBuilder.Add(MakeTypeArgInfo(namedTypeArg));
                                }
                            }

                            fieldsBuilder.Add(new AotFieldInfo {
                                fieldTypeFullName = fieldTypeFullName,
                                fieldTypeCanEnsure = fieldTypeCanEnsure,
                                collectionKind = AotCollectionKind.None,
                                otherTypeArgs = otherTypeArgsBuilder.ToImmutable().AsEquatableArray(),
                            });
                        }

                        typeWalker = typeWalker.BaseType;
                    }
                }

                builder.Add(new AotTypeCandidate {
                    fullName = type.ToFullName(),
                    fields = fieldsBuilder.ToImmutable().AsEquatableArray(),
                });
            }
        }

        private static AotTypeArgInfo MakeTypeArgInfo(INamedTypeSymbol typeArg)
        {
            return new AotTypeArgInfo {
                fullName = typeArg.ToFullName(),
                canEnsure = CanEnsureType(typeArg),
            };
        }

        private static bool CanEnsureType(INamedTypeSymbol type)
        {
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
            , NewtonsoftAotHelperInfo helperInfo
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
                // Reconstruct hintName using the same formula as
                // SyntaxNodeExtensions.GetGeneratedSourceFileName(syntaxTree, generatorName, salting, typeName).
                var locationFilePath = helperInfo.location.filePath;
                var fileBaseName = Path.GetFileNameWithoutExtension(locationFilePath);
                var stableHashCode = SourceGenHelpers.GetStableHashCode(locationFilePath) & 0x7fffffff;
                var lineNumber = helperInfo.location.startLine;
                var hintName = $"{fileBaseName}__{helperInfo.fileHintName}_{stableHashCode}_{lineNumber}.g.cs";

                SourceGenHelpers.ProjectPath = projectPath;
                var sourceFilePath = BuildSourceFilePath(compilation.assemblyName, hintName, projectPath);

                var source = HelperDeclaration.WriteCode(helperInfo, helperInfo.typeCandidates.AsImmutableArray());

                var sourceText = SourceText.From(source, Encoding.UTF8)
                    .WithIgnoreUnassignedVariableWarning()
                    .WithInitialLineDirectiveToGeneratedSource(sourceFilePath);

                context.AddSource(hintName, sourceText);

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , helperInfo.location.ToLocation()
                        , sourceFilePath
                        , sourceText
                        , projectPath
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }
            }
        }

        private static string BuildSourceFilePath(
              string assemblyName
            , string hintName
            , string projectPath = null
        )
        {
            if (projectPath is not null)
            {
                var dir = $"{projectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var dir = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(dir);
                return $"{dir}{hintName}";
            }

            return $"Temp/GeneratedCode/{assemblyName}/{hintName}";
        }
    }
}
