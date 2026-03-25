using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    [Generator]
    internal class UserDataVaultGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(UserDataVaultGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var providerClassProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  VAULT_ATTRIBUTE_METADATA
                , static (node, _) => node is ClassDeclarationSyntax syntax
                      && syntax.HasModifier(SyntaxKind.AbstractKeyword) == false
                      && syntax.TypeParameterList is null
                , GetVaultInfo
            ).Where(static t => t.IsValid);

            var accessProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  ACCESSOR_ATTRIBUTE_METADATA
                , static (node, _) => node is ClassDeclarationSyntax syntax
                      && syntax.HasModifier(SyntaxKind.AbstractKeyword) == false
                      && syntax.TypeParameterList is null
                , GetAccessorInfo
            ).Where(static t => t.isValid);

            var combined = providerClassProvider
                .Combine(accessProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static UserDataVaultInfo GetVaultInfo(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
            )
            {
                return default;
            }

            var fileHintName = symbol.ToFileName();
            var syntaxTree = context.TargetNode.SyntaxTree;
            var containingNs = symbol.ContainingNamespace;

            return new UserDataVaultInfo {
                location = LocationInfo.From(context.TargetNode.GetLocation()),
                metadataName = symbol.ToSimpleName(),
                className = symbol.Name,
                isStatic = symbol.IsStatic,
                namespaceName = containingNs is { IsGlobalNamespace: false }
                    ? containingNs.ToDisplayString()
                    : string.Empty,
                containingTypeDeclarations = symbol.GetContainingTypes(),
                fileHintName = fileHintName,
                sourceHintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, context.TargetNode, fileHintName),
            };
        }

        private static UserDataAccessorInfo GetAccessorInfo(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
            )
            {
                return default;
            }

            // Extract target vault from the attribute's first constructor argument (if any)
            var vaultMetadataName = string.Empty;
            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length > 0
                && attribute.ConstructorArguments[0].Value is INamedTypeSymbol vaultType
            )
            {
                vaultMetadataName = vaultType.ToSimpleName();
            }

            // Extract field name from [Label] or [DisplayName] attribute, fallback to class name
            var fieldName = string.Empty;

            foreach (var attrib in symbol.GetAttributes())
            {
                var attribName = attrib.AttributeClass?.Name ?? string.Empty;

                if (attribName is not ("LabelAttribute" or "DisplayNameAttribute"))
                {
                    continue;
                }

                if (attrib.ConstructorArguments.Length > 0)
                {
                    var arg = attrib.ConstructorArguments[0];

                    if (arg.Kind == TypedConstantKind.Primitive && arg.Value?.ToString() is string dn)
                    {
                        fieldName = dn;
                        goto NEXT;
                    }
                }
                else if (attrib.NamedArguments.Length > 0)
                {
                    foreach (var arg in attrib.NamedArguments)
                    {
                        if (arg.Key is "Name" or "DisplayName"
                            && arg.Value.Kind == TypedConstantKind.Primitive
                            && arg.Value.Value?.ToString() is string dn
                        )
                        {
                            fieldName = dn;
                            goto NEXT;
                        }
                    }
                }
            }

            NEXT:

            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = symbol.Name;
            }

            // Find constructor with the most parameters
            var constructors = symbol.Constructors;
            var constructorIndex = -1;
            var max = 0;

            for (var i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].Parameters.Length > max)
                {
                    max = constructors[i].Parameters.Length;
                    constructorIndex = i;
                }
            }

            if (constructorIndex != 0)
            {
                return default;
            }

            var constructor = constructors[constructorIndex];
            var parameters = constructor.Parameters;
            using var builder = ImmutableArrayBuilder<AccessorArgInfo>.Rent();
            var isValid = true;

            foreach (var param in parameters)
            {
                if (ParamDefinition.TryGetParam(param.Type, out var argType))
                {
                    var dataTypeHasDefaultConstructor = false;

                    if (argType != null)
                    {
                        var nonDefaultCount = 0;
                        var defaultCount = 0;

                        foreach (var member in argType.GetMembers())
                        {
                            if (member is IMethodSymbol method
                                && method.MethodKind == MethodKind.Constructor
                            )
                            {
                                if (method.Parameters.Length > 0)
                                    nonDefaultCount++;
                                else
                                    defaultCount++;
                            }
                        }

                        dataTypeHasDefaultConstructor = defaultCount > 0 || nonDefaultCount < 1;
                    }

                    builder.Add(new AccessorArgInfo(
                          isStore: argType != null
                        , fullTypeName: param.Type.ToFullName()
                        , fullDataTypeName: argType?.ToFullName() ?? string.Empty
                        , typeName: argType?.Name ?? param.Type.Name
                        , dataTypeHasDefaultConstructor: dataTypeHasDefaultConstructor
                    ));
                }
                else
                {
                    isValid = false;
                }
            }

            return new UserDataAccessorInfo {
                location = LocationInfo.From(context.TargetNode.GetLocation()),
                metadataName = symbol.ToSimpleName(),
                vaultMetadataName = vaultMetadataName,
                fieldName = fieldName,
                symbolName = symbol.Name,
                args = builder.ToImmutable().AsEquatableArray(),
                isInitializable = symbol.InheritsFromInterface("global::EncosyTower.Initialization.IInitializable"),
                isDeinitializable = symbol.InheritsFromInterface("global::EncosyTower.Initialization.IDeinitializable"),
                isValid = isValid,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilationCandidate
            , UserDataVaultInfo vaultInfo
            , ImmutableArray<UserDataAccessorInfo> accessorInfos
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (vaultInfo.IsValid == false || accessorInfos.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var accessDeclarations = new List<UserDataAccessorDefinition>(accessorInfos.Length);

            for (var i = 0; i < accessorInfos.Length; i++)
            {
                var aInfo = accessorInfos[i];

                if (string.IsNullOrEmpty(aInfo.vaultMetadataName) == false
                    && string.Equals(aInfo.vaultMetadataName, vaultInfo.metadataName, StringComparison.Ordinal) == false
                )
                {
                    continue;
                }

                var accessDeclaration = new UserDataAccessorDefinition(aInfo);

                if (accessDeclaration.IsValid)
                {
                    accessDeclarations.Add(accessDeclaration);
                }
            }

            if (accessDeclarations.Count < 1)
            {
                return;
            }

            accessDeclarations.Sort(static (x, y) => {
                return string.Compare(x.SymbolName, y.SymbolName, StringComparison.Ordinal);
            });

            var declaration = new UserDataVaultDeclaration(
                  vaultInfo.className
                , vaultInfo.isStatic
                , accessDeclarations
            );

            SourceGenHelpers.ProjectPath = projectPath;

            // Build the opening source from pre-extracted namespace / containing type declarations
            // (replaces TypeCreationHelpers.GenerateOpeningAndClosingSource which requires a SyntaxNode).
            var openingPrinter = Printer.DefaultLarge;
            var printUsings = compilationCandidate.references.unitask
                ? (PrinterAction)PrintUsingUniTask : PrintUsingAwaitable;
            printUsings(ref openingPrinter);

            var hasNamespace = string.IsNullOrEmpty(vaultInfo.namespaceName) == false;

            if (hasNamespace)
            {
                openingPrinter.PrintLine($"namespace {vaultInfo.namespaceName}");
                openingPrinter.OpenScope();
            }

            var containingTypes = vaultInfo.containingTypeDeclarations;

            for (var i = 0; i < containingTypes.Count; i++)
            {
                openingPrinter.PrintLine(containingTypes[i]);
                openingPrinter.OpenScope();
            }

            var openingSource = openingPrinter.Result;

            // Build the closing source with matching closing braces.
            var closingPrinter = Printer.DefaultLarge;
            closingPrinter.PrintEndLine();

            var closingDepth = containingTypes.Count + (hasNamespace ? 1 : 0);

            for (var i = 0; i < closingDepth; i++)
            {
                closingPrinter = closingPrinter.DecreasedIndent();
                closingPrinter.PrintLine("}");
            }

            var closingSource = closingPrinter.Result;

            // Compute source file path without accessing SyntaxTree
            // (mirrors SyntaxNodeExtensions.GetGeneratedSourceFilePath logic).
            var stableHashCode = SourceGenHelpers.GetStableHashCode(vaultInfo.location.filePath) & 0x7fffffff;
            var fileHintName = vaultInfo.fileHintName;
            var sourceFileName = $"{fileHintName}__{GENERATOR_NAME}_{stableHashCode}_0.g.cs";
            var sourceFilePath = SourceGenHelpers.CanWriteToProjectPath
                ? $"{projectPath}/Temp/GeneratedCode/{compilationCandidate.assemblyName}/{sourceFileName}"
                : $"Temp/GeneratedCode/{compilationCandidate.assemblyName}/{sourceFileName}";

            context.OutputSource(
                  outputSourceGenFiles
                , openingSource
                , declaration.WriteCode()
                , closingSource
                , vaultInfo.sourceHintName
                , sourceFilePath
                , vaultInfo.location.ToLocation()
                , projectPath
            );

            return;

            static void PrintUsingUniTask(ref Printer p)
            {
                p.PrintEndLine();
                p.PrintLine("using UnityTask = global::Cysharp.Threading.Tasks.UniTask;");
                p.PrintLine("using UnityTaskᐸboolᐳ = global::Cysharp.Threading.Tasks.UniTask<bool>;");
                PrintAdditionalUsings(ref p);
            }

            static void PrintUsingAwaitable(ref Printer p)
            {
                p.PrintEndLine();
                p.PrintLine("using UnityTask = global::UnityEngine.Awaitable;");
                p.PrintLine("using UnityTaskᐸboolᐳ = global::UnityEngine.Awaitable<bool>;");
                PrintAdditionalUsings(ref p);
            }

            static void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintLine("using CancellationToken = global::System.Threading.CancellationToken;");
                p.PrintLine("using DoesNotReturnIfAttribute = global::System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute;");
                p.PrintLine("using EncryptionBase = global::EncosyTower.Encryption.EncryptionBase;");
                p.PrintLine("using ExcludeFromCodeCoverageAttribute = global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute;");
                p.PrintLine("using HideInCallstackAttribute = global::UnityEngine.HideInCallstackAttribute;");
                p.PrintLine("using GeneratedCodeAttribute = global::System.CodeDom.Compiler.GeneratedCodeAttribute;");
                p.PrintLine("using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;");
                p.PrintLine("using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;");
                p.PrintLine("using NotNullAttribute = global::System.Diagnostics.CodeAnalysis.NotNullAttribute;");
                p.PrintLine("using SaveDestination = global::EncosyTower.UserDataVaults.SaveDestination;");
                p.PrintLine("using SerializableAttribute = global::System.SerializableAttribute;");
                p.PrintLine("using SerializeField = global::UnityEngine.SerializeField;");
                p.PrintLine("using SpanᐸStringIdᐸstringᐳᐳ = global::System.Span<global::EncosyTower.StringIds.StringId<string>>;");
                p.PrintLine("using SpanᐸIUserDataᐳ = global::System.Span<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using SpanᐸIUserDataAccessorᐳ = global::System.Span<global::EncosyTower.UserDataVaults.IUserDataAccessor>;");
                p.PrintLine("using StackTraceHiddenAttribute = global::System.Diagnostics.StackTraceHiddenAttribute;");
                p.PrintLine("using StringIdᐸstringᐳ = global::EncosyTower.StringIds.StringId<string>;");
                p.PrintLine("using StringVault = global::EncosyTower.StringIds.StringVault;");
                p.PrintLine("using SourcePriority = global::EncosyTower.UserDataVaults.SourcePriority;");
                p.PrintLine("using ThrowHelper = global::EncosyTower.Debugging.ThrowHelper;");
                p.PrintLine("using UnityTasks = global::EncosyTower.Tasks.UnityTasks;");
                p.PrintLine("using UserDataStoreArgs = global::EncosyTower.UserDataVaults.UserDataStoreArgs;");
                p.PrintLine("using UserDataVaultBase = global::EncosyTower.UserDataVaults.UserDataVaultBase;");
                p.PrintEndLine();

                p.PrintLine("using IDeinitializable = global::EncosyTower.Initialization.IDeinitializable;");
                p.PrintLine("using IDisposable = global::System.IDisposable;");
                p.PrintLine("using IEnumerable = global::System.Collections.IEnumerable;");
                p.PrintLine("using IEnumerator = global::System.Collections.IEnumerator;");
                p.PrintLine("using IEnumerableᐸStringIdᐸstringᐳᐳ = global::System.Collections.Generic.IEnumerable<global::EncosyTower.StringIds.StringId<string>>;");
                p.PrintLine("using IEnumeratorᐸStringIdᐸstringᐳᐳ = global::System.Collections.Generic.IEnumerator<global::EncosyTower.StringIds.StringId<string>>;");
                p.PrintLine("using IEnumerableᐸIUserDataᐳ = global::System.Collections.Generic.IEnumerable<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using IEnumeratorᐸIUserDataᐳ = global::System.Collections.Generic.IEnumerator<global::EncosyTower.UserDataVaults.IUserData>;");
                p.PrintLine("using IEnumerableᐸIUserDataAccessorᐳ = global::System.Collections.Generic.IEnumerable<global::EncosyTower.UserDataVaults.IUserDataAccessor>;");
                p.PrintLine("using IEnumeratorᐸIUserDataAccessorᐳ = global::System.Collections.Generic.IEnumerator<global::EncosyTower.UserDataVaults.IUserDataAccessor>;");
                p.PrintLine("using IInitializable = global::EncosyTower.Initialization.IInitializable;");
                p.PrintLine("using IIsCreated = global::EncosyTower.Common.IIsCreated;");
                p.PrintLine("using ILogger = global::EncosyTower.Logging.ILogger;");
                p.PrintLine("using IUserData = global::EncosyTower.UserDataVaults.IUserData;");
                p.PrintLine("using IUserDataAccessor = global::EncosyTower.UserDataVaults.IUserDataAccessor;");
                p.PrintLine("using IUserDataAccessorCollection = global::EncosyTower.UserDataVaults.IUserDataAccessorCollection;");
                p.PrintLine("using IUserDataAccessorReadOnlyCollection = global::EncosyTower.UserDataVaults.IUserDataAccessorReadOnlyCollection;");
                p.PrintLine("using IUserDataCollection = global::EncosyTower.UserDataVaults.IUserDataCollection;");
                p.PrintLine("using IUserDataDirectory = global::EncosyTower.UserDataVaults.IUserDataDirectory;");
                p.PrintLine("using IUserDataIdCollection = global::EncosyTower.UserDataVaults.IUserDataIdCollection;");
            }
        }

    }
}
