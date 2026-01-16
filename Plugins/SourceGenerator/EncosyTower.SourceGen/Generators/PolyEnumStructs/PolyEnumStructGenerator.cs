using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    [Generator]
    internal class PolyEnumStructGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string POLY_ENUM_STRUCT = "PolyEnumStruct";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string CONSTRUCT_ENUM_CASE_FROM_ATTRIBUTE = $"global::{NAMESPACE}.ConstructEnumCaseFromAttribute";
        private const string GENERATOR_NAME = nameof(PolyEnumStructGenerator);
        private const string UNDEFINED_NAME = "Undefined";
        private const string INTERFACE_NAME = "IEnumCase";
        private const string READ_ONLY_ATTRIBUTE = "global::System.ComponentModel.ReadOnlyAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidStructSyntax,
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

        private static bool IsValidStructSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is StructDeclarationSyntax syntax
                && syntax.TypeParameterList is null
                && syntax.AttributeLists.Count > 0
                && syntax.GetAttribute(NAMESPACE, POLY_ENUM_STRUCT) is not null
                ;
        }

        private static PolyEnumStructDefinition GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax structSyntax
                || structSyntax.TypeParameterList is not null
                || structSyntax.GetAttribute(NAMESPACE, POLY_ENUM_STRUCT) is null
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(structSyntax, token);

            if (symbol is not INamedTypeSymbol structSymbol)
            {
                return default;
            }

            var attribute = symbol.GetAttribute(POLY_ENUM_STRUCT_ATTRIBUTE);

            if (attribute == null)
            {
                return default;
            }

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = structSyntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var fileTypeName = structSymbol.ToFileName();
            var hintName = syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, structSyntax, fileTypeName);
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME, fileTypeName);
            var symbolUniqueSet = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  structSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var result = new PolyEnumStructDefinition {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                location = structSyntax.GetLocation(),
                parentIsNamespace = structSyntax.Parent is BaseNamespaceDeclarationSyntax,
            };

            foreach (var arg in attribute.NamedArguments)
            {
                if (arg.Key == "SortFieldsBySize" && arg.Value.Value is bool sortFieldsBySize)
                {
                    result.sortFieldsBySize = sortFieldsBySize;
                }
                else if (arg.Key == "AutoEquatable" && arg.Value.Value is bool autoEquatable)
                {
                    result.autoEquatable = autoEquatable;
                }
                else if (arg.Key == "WithEnumExtensions" && arg.Value.Value is bool withEnumExtensions)
                {
                    result.withEnumExtensions = withEnumExtensions;
                }
            }

            AggregateInterfaceAndStructs(structSymbol, ref result, semanticModel, token, symbolUniqueSet);

            return result;

            static void PrintAdditionalUsings(ref Printer p)
            {
                p.PrintEndLine();
                p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
                p.PrintLine("using System;");
                p.PrintLine("using System.CodeDom.Compiler;");
                p.PrintLine("using System.Diagnostics;");
                p.PrintLine("using System.Diagnostics.CodeAnalysis;");
                p.PrintLine("using System.Runtime.CompilerServices;");
                p.PrintLine("using System.Runtime.InteropServices;");
                p.PrintLine("using EncosyTower.Common;");
                p.PrintLine("using UnityEngine;");
                p.PrintEndLine();
                p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
                p.PrintEndLine();
            }

            static void AggregateInterfaceAndStructs(
                  INamedTypeSymbol structSymbol
                , ref PolyEnumStructDefinition polyEnumStruct
                , SemanticModel semanticModel
                , CancellationToken token
                , HashSet<ISymbol> symbolUniqueSet
            )
            {
                using var structsBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.StructDefinition>.Rent();

                var structName = structSymbol.Name;
                var verboseUndefinedName = $"{structName}_{UNDEFINED_NAME}";

                PolyEnumStructDefinition.StructDefinition undefinedStruct = default;

                foreach (var syntaxRef in structSymbol.DeclaringSyntaxReferences)
                {
                    token.ThrowIfCancellationRequested();

                    if (syntaxRef.GetSyntax(token) is not StructDeclarationSyntax parentSyntax)
                    {
                        continue;
                    }

                    foreach (var childNode in parentSyntax.ChildNodes())
                    {
                        if (childNode is StructDeclarationSyntax structSyntax
                            && structSyntax.TypeParameterList is null
                        )
                        {
                            var @struct = GetStruct(structSyntax, semanticModel, token, symbolUniqueSet);

                            if (@struct.IsValid == false)
                            {
                                continue;
                            }

                            if (polyEnumStruct.definedUndefinedStruct == PolyEnumStructDefinition.DefinedUndefinedStruct.None
                                && TryGetUndefinedCase(@struct.name, structName, UNDEFINED_NAME, verboseUndefinedName, out var result)
                            )
                            {
                                undefinedStruct = @struct;
                                polyEnumStruct.definedUndefinedStruct = result;
                            }
                            else
                            {
                                structsBuilder.Add(@struct);
                            }
                        }
                        else if (childNode is InterfaceDeclarationSyntax interfaceSyntax
                            && interfaceSyntax.TypeParameterList is null
                            && string.Equals(interfaceSyntax.Identifier.Text, INTERFACE_NAME, StringComparison.Ordinal)
                        )
                        {
                            polyEnumStruct.interfaceDef = GetInterface(interfaceSyntax, semanticModel, token);
                        }
                    }
                }

                if (undefinedStruct.IsValid)
                {
                    structsBuilder.Add(undefinedStruct with {
                        isUndefined = true,
                        implicitlyDeclared = false,
                    });
                }
                else if (polyEnumStruct.definedUndefinedStruct == PolyEnumStructDefinition.DefinedUndefinedStruct.None)
                {
                    structsBuilder.Add(new PolyEnumStructDefinition.StructDefinition {
                        name = verboseUndefinedName,
                        identifier = UNDEFINED_NAME,
                        isUndefined = true,
                        implicitlyDeclared = true,
                    });
                }

                polyEnumStruct.structs = structsBuilder.ToImmutable();
                EnsureInterface(ref polyEnumStruct.interfaceDef);
            }

            static bool TryGetUndefinedCase(
                  string caseName
                , string structName
                , string undefinedName
                , string verboseUndefinedName
                , out PolyEnumStructDefinition.DefinedUndefinedStruct result
            )
            {
                if (string.Equals(caseName, undefinedName, StringComparison.Ordinal))
                {
                    result = PolyEnumStructDefinition.DefinedUndefinedStruct.Default;
                    return true;
                }

                if (string.Equals(caseName, verboseUndefinedName, StringComparison.Ordinal))
                {
                    result = PolyEnumStructDefinition.DefinedUndefinedStruct.Verbose;
                    return true;
                }

                result = default;
                return false;
            }

            static PolyEnumStructDefinition.InterfaceDefinition GetInterface(
                  InterfaceDeclarationSyntax syntax
                , SemanticModel semanticModel
                , CancellationToken token
            )
            {
                var result = new PolyEnumStructDefinition.InterfaceDefinition {
                    name = INTERFACE_NAME,
                    definedInterface = true,
                };

                var members = syntax.Members;

                if (members.Count > 0)
                {
                    token.ThrowIfCancellationRequested();

                    var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

                    AggregateInterfaceMembers(symbol, ref result, token);
                }

                return result;
            }

            static void AggregateInterfaceMembers(
                  INamedTypeSymbol symbol
                , ref PolyEnumStructDefinition.InterfaceDefinition interfaceDef
                , CancellationToken token
            )
            {
                using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.PropertyDefinition>.Rent();
                using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.IndexerDefinition>.Rent();
                using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.MethodDefinition>.Rent();

                foreach (var member in symbol.GetMembers())
                {
                    if (TryGetInterfaceMember(member, propertiesBuilder, indexersBuilder, methodsBuilder, token) == false)
                    {
                        propertiesBuilder.Clear();
                        indexersBuilder.Clear();
                        methodsBuilder.Clear();
                        break;
                    }
                }

                interfaceDef.properties = propertiesBuilder.ToImmutable();
                interfaceDef.indexers = indexersBuilder.ToImmutable();
                interfaceDef.methods = methodsBuilder.ToImmutable();
            }

            static void EnsureInterface(ref PolyEnumStructDefinition.InterfaceDefinition result)
            {
                if (result.IsValid)
                {
                    return;
                }

                using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.PropertyDefinition>.Rent();
                using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.IndexerDefinition>.Rent();
                using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.MethodDefinition>.Rent();

                result = new PolyEnumStructDefinition.InterfaceDefinition {
                    name = INTERFACE_NAME,
                    properties = propertiesBuilder.ToImmutable(),
                    indexers = indexersBuilder.ToImmutable(),
                    methods = methodsBuilder.ToImmutable(),
                    definedInterface = false,
                };
            }

            static PolyEnumStructDefinition.StructDefinition GetStruct(
                  StructDeclarationSyntax syntax
                , SemanticModel semanticModel
                , CancellationToken token
                , HashSet<ISymbol> symbolUniqueSet
            )
            {
                token.ThrowIfCancellationRequested();

                var symbol = semanticModel.GetDeclaredSymbol(syntax, token);
                var result = new PolyEnumStructDefinition.StructDefinition {
                    name = syntax.Identifier.Text,
                    displayName = syntax.GetDisplayNameOrDefault(syntax.Identifier.Text),
                    identifier = syntax.Identifier.Text.ToValidIdentifier(),
                    isUndefined = false,
                };

                AggregateConstructions(symbol, ref result);
                AggregateStructMembers(symbol, ref result);

                return result;
            }

            static void AggregateConstructions(
                  INamedTypeSymbol symbol
                , ref PolyEnumStructDefinition.StructDefinition structDef
            )
            {
                using var arrayBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.ConstructionDefinition>.Rent();

                foreach (var attribute in symbol.GetAttributes(CONSTRUCT_ENUM_CASE_FROM_ATTRIBUTE))
                {
                    if (attribute.ConstructorArguments.Length < 1)
                    {
                        continue;
                    }

                    var arg = attribute.ConstructorArguments[0];

                    if (arg.IsNull == false
                        && arg.Kind is TypedConstantKind.Primitive or TypedConstantKind.Enum
                        && arg.Type is INamedTypeSymbol typeSymbol
                    )
                    {
                        arrayBuilder.Add(new PolyEnumStructDefinition.ConstructionDefinition {
                            type = GetSlimType(arg.Type),
                            value = GetCreationValue(arg.Type, arg.Value),
                        });
                    }
                }

                structDef.constructions = arrayBuilder.ToImmutable();
            }

            static void AggregateStructMembers(
                  INamedTypeSymbol symbol
                , ref PolyEnumStructDefinition.StructDefinition structDef
            )
            {
                using var fieldsBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.FieldDefinition>.Rent();
                using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.PropertyDefinition>.Rent();
                using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.IndexerDefinition>.Rent();
                using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.MethodDefinition>.Rent();

                int structSize = 0;

                foreach (var member in symbol.GetMembers())
                {
                    if (member.IsStatic)
                    {
                        continue;
                    }

                    var accessibility = member.DeclaredAccessibility;

                    if (member is IFieldSymbol fieldSymbol)
                    {
                        if (fieldSymbol.IsConst
                            || ((fieldSymbol.IsImplicitlyDeclared && accessibility is Accessibility.Private) == false
                            && (accessibility is Accessibility.Public or Accessibility.Internal) == false
                            )
                        )
                        {
                            continue;
                        }

                        string fieldName;

                        if (fieldSymbol.IsImplicitlyDeclared && accessibility is Accessibility.Private)
                        {
                            var displayPropertyName = fieldSymbol.ToDisplayParts()
                                .Where(static x => x.Kind == SymbolDisplayPartKind.PropertyName)
                                .FirstOrDefault();

                            if (displayPropertyName.Kind == SymbolDisplayPartKind.PropertyName)
                            {
                                fieldName = displayPropertyName.ToString();
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            fieldName = fieldSymbol.Name;
                        }

                        int fieldSize = 0;
                        fieldSymbol.Type.GetUnmanagedSize(ref fieldSize);
                        structSize += fieldSize;

                        fieldsBuilder.Add(new PolyEnumStructDefinition.FieldDefinition {
                            name = fieldName,
                            returnType = GetSlimType(fieldSymbol.Type),
                            size = fieldSize,
                            implicityDeclared = fieldSymbol.IsImplicitlyDeclared,
                            isReadOnly = fieldSymbol.IsReadOnly,
                        });

                        continue;
                    }

                    if (member.IsImplicitlyDeclared == false
                        && accessibility is Accessibility.Public or Accessibility.Internal
                    )
                    {
                        GetStructMember(member, propertiesBuilder, indexersBuilder, methodsBuilder);
                    }
                }

                structDef.fields = fieldsBuilder.ToImmutable();
                structDef.properties = propertiesBuilder.ToImmutable();
                structDef.indexers = indexersBuilder.ToImmutable();
                structDef.methods = methodsBuilder.ToImmutable();
            }

            static bool TryGetInterfaceMember(
                  ISymbol member
                , ImmutableArrayBuilder<PolyEnumStructDefinition.PropertyDefinition> propertiesBuilder
                , ImmutableArrayBuilder<PolyEnumStructDefinition.IndexerDefinition> indexersBuilder
                , ImmutableArrayBuilder<PolyEnumStructDefinition.MethodDefinition> methodsBuilder
                , CancellationToken token
            )
            {
                if (member is IPropertySymbol propertySymbol)
                {
                    var getterIsDim = false;
                    var setterIsDim = false;

                    foreach (var syntaxRef in propertySymbol.DeclaringSyntaxReferences)
                    {
                        token.ThrowIfCancellationRequested();

                        if (syntaxRef.GetSyntax(token) is not BasePropertyDeclarationSyntax baseSyntax)
                        {
                            continue;
                        }

                        if (baseSyntax.AccessorList is AccessorListSyntax accessorListSyntax
                            && accessorListSyntax.Accessors.Count > 0
                        )
                        {
                            foreach (var accessor in accessorListSyntax.Accessors)
                            {
                                if (accessor.Body is not null || accessor.ExpressionBody is not null)
                                {
                                    if (accessor.Keyword.Text == "get")
                                    {
                                        getterIsDim = true;
                                    }
                                    else if (accessor.Keyword.Text == "set")
                                    {
                                        setterIsDim = true;
                                    }
                                }
                            }
                        }
                        else if (baseSyntax is PropertyDeclarationSyntax propertySyntax)
                        {
                            if (propertySyntax.ExpressionBody is not null)
                            {
                                getterIsDim = true;
                            }
                        }
                        else if (baseSyntax is IndexerDeclarationSyntax indexerSyntax)
                        {
                            if (indexerSyntax.ExpressionBody is not null)
                            {
                                getterIsDim = true;
                            }
                        }
                    }

                    var isReadOnly = false;

                    if (propertySymbol.TryGetAttribute(READ_ONLY_ATTRIBUTE, out var attrib)
                        && attrib.ConstructorArguments.Length == 1
                    )
                    {
                        isReadOnly = (bool)attrib.ConstructorArguments[0].Value;
                    }

                    if (propertySymbol.IsIndexer)
                    {
                        var indexerDef = new PolyEnumStructDefinition.IndexerDefinition {
                            returnType = GetSlimType(propertySymbol.Type),
                            getter = GetPropertyMethod(propertySymbol.GetMethod, isGetter: true, getterIsDim),
                            setter = GetPropertyMethod(propertySymbol.SetMethod, isGetter: false, setterIsDim),
                            refKind = propertySymbol.RefKind,
                        };

                        using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.ParameterDefinition>.Rent();

                        foreach (var parameter in propertySymbol.Parameters)
                        {
                            parametersBuilder.Add(new PolyEnumStructDefinition.ParameterDefinition {
                                name = parameter.Name,
                                type = GetSlimType(parameter.Type),
                                refKind = parameter.RefKind,
                            });
                        }

                        indexerDef.parameters = parametersBuilder.ToImmutable();

                        if (isReadOnly)
                        {
                            indexerDef.getter.isReadOnly = isReadOnly;
                            indexerDef.setter = default;
                        }

                        indexersBuilder.Add(indexerDef);
                    }
                    else
                    {
                        var propertyDef = new PolyEnumStructDefinition.PropertyDefinition {
                            name = propertySymbol.Name,
                            returnType = GetSlimType(propertySymbol.Type),
                            refKind = propertySymbol.RefKind,
                            getter = GetPropertyMethod(propertySymbol.GetMethod, isGetter: true, getterIsDim),
                            setter = GetPropertyMethod(propertySymbol.SetMethod, isGetter: false, setterIsDim),
                        };

                        if (isReadOnly)
                        {
                            propertyDef.getter.isReadOnly = isReadOnly;
                            propertyDef.setter = default;
                        }

                        propertiesBuilder.Add(propertyDef);
                    }
                }
                else if (member is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Ordinary)
                {
                    if (methodSymbol.TypeParameters.Length > 0)
                    {
                        return false;
                    }

                    var isDim = false;

                    foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
                    {
                        token.ThrowIfCancellationRequested();

                        if (syntaxRef.GetSyntax(token) is MethodDeclarationSyntax methodSyntax)
                        {
                            isDim = methodSyntax.Body is not null || methodSyntax.ExpressionBody is not null;
                        }
                    }

                    var methodDef = new PolyEnumStructDefinition.MethodDefinition {
                        name = methodSymbol.Name,
                        returnType = GetSlimType(methodSymbol.ReturnType),
                        refKind = methodSymbol.RefKind,
                        returnsVoid = methodSymbol.ReturnsVoid,
                        isReadOnly = methodSymbol.IsReadOnly,
                        isDim = isDim,
                    };

                    using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.ParameterDefinition>.Rent();

                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        parametersBuilder.Add(new PolyEnumStructDefinition.ParameterDefinition {
                            name = parameter.Name,
                            type = GetSlimType(parameter.Type),
                            refKind = parameter.RefKind,
                        });
                    }

                    methodDef.parameters = parametersBuilder.ToImmutable();
                    methodsBuilder.Add(methodDef);
                }

                return true;
            }

            static void GetStructMember(
                  ISymbol member
                , ImmutableArrayBuilder<PolyEnumStructDefinition.PropertyDefinition> propertiesBuilder
                , ImmutableArrayBuilder<PolyEnumStructDefinition.IndexerDefinition> indexersBuilder
                , ImmutableArrayBuilder<PolyEnumStructDefinition.MethodDefinition> methodsBuilder
            )
            {
                if (member is IPropertySymbol propertySymbol)
                {
                    if (propertySymbol.ExplicitInterfaceImplementations.Length > 0)
                    {
                        return;
                    }

                    if (propertySymbol.IsIndexer)
                    {
                        var indexerDef = new PolyEnumStructDefinition.IndexerDefinition {
                            returnType = GetSlimType(propertySymbol.Type),
                            refKind = propertySymbol.RefKind,
                            getter = GetPropertyMethod(propertySymbol.GetMethod, isGetter: true, false),
                            setter = GetPropertyMethod(propertySymbol.SetMethod, isGetter: false, false),
                        };

                        using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.ParameterDefinition>.Rent();

                        foreach (var parameter in propertySymbol.Parameters)
                        {
                            parametersBuilder.Add(new PolyEnumStructDefinition.ParameterDefinition {
                                name = parameter.Name,
                                type = GetSlimType(parameter.Type),
                                refKind = parameter.RefKind,
                            });
                        }

                        indexerDef.parameters = parametersBuilder.ToImmutable();
                        indexersBuilder.Add(indexerDef);
                    }
                    else
                    {
                        propertiesBuilder.Add(new PolyEnumStructDefinition.PropertyDefinition {
                            name = propertySymbol.Name,
                            returnType = GetSlimType(propertySymbol.Type),
                            refKind = propertySymbol.RefKind,
                            getter = GetPropertyMethod(propertySymbol.GetMethod, isGetter: true, false),
                            setter = GetPropertyMethod(propertySymbol.SetMethod, isGetter: false, false),
                        });
                    }
                }
                else if (member is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Ordinary)
                {
                    if (methodSymbol.ExplicitInterfaceImplementations.Length > 0
                        || methodSymbol.TypeParameters.Length > 0
                    )
                    {
                        return;
                    }

                    var methodDef = new PolyEnumStructDefinition.MethodDefinition {
                        name = methodSymbol.Name,
                        returnType = GetSlimType(methodSymbol.ReturnType),
                        refKind = methodSymbol.RefKind,
                        returnsVoid = methodSymbol.ReturnsVoid,
                        isReadOnly = methodSymbol.IsReadOnly,
                    };

                    using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructDefinition.ParameterDefinition>.Rent();

                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        parametersBuilder.Add(new PolyEnumStructDefinition.ParameterDefinition {
                            name = parameter.Name,
                            type = GetSlimType(parameter.Type),
                            refKind = parameter.RefKind,
                        });
                    }

                    methodDef.parameters = parametersBuilder.ToImmutable();
                    methodsBuilder.Add(methodDef);
                }
            }

            static PolyEnumStructDefinition.SlimTypeDefinition GetSlimType(ITypeSymbol typeSymbol)
            {
                return new PolyEnumStructDefinition.SlimTypeDefinition {
                    name = typeSymbol.ToFullName(),
                    isEnum = typeSymbol.IsEnumType(),
                };
            }

            static string GetCreationValue(ITypeSymbol typeSymbol, object value)
            {
                if (typeSymbol.TypeKind == TypeKind.Enum)
                {
                    return typeSymbol.GetEnumMemberName(value);
                }

                if (typeSymbol.SpecialType == SpecialType.System_String)
                {
                    return $"\"{value}\"";
                }

                return value.ToString();
            }

            static PolyEnumStructDefinition.PropertyMethodDefinition GetPropertyMethod(
                  IMethodSymbol methodSymbol
                , bool isGetter
                , bool isDim
            )
            {
                if (methodSymbol is null)
                {
                    return default;
                }

                var isReadOnly = methodSymbol.IsReadOnly;

                if (methodSymbol.TryGetAttribute(READ_ONLY_ATTRIBUTE, out var attrib)
                    && attrib.ConstructorArguments.Length == 1
                )
                {
                    isReadOnly = (bool)attrib.ConstructorArguments[0].Value;
                }

                return new PolyEnumStructDefinition.PropertyMethodDefinition {
                    isValid = true,
                    refKind = methodSymbol.RefKind,
                    isGetter = isGetter,
                    isReadOnly = isReadOnly,
                    isDim = isDim,
                };
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
            , PolyEnumStructDefinition candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , candidate.WriteCode(compilation.references.unityCollections)
                    , candidate.closingSource
                    , candidate.hintName
                    , candidate.sourceFilePath
                    , candidate.location
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
                    , candidate.location
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_POLY_ENUM_STRUCT_01"
                , "Poly Enum Struct Generator Error"
                , "This error indicates a bug in the Poly Enum Struct source generators. Error message: '{0}'."
                , POLY_ENUM_STRUCT_ATTRIBUTE
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
