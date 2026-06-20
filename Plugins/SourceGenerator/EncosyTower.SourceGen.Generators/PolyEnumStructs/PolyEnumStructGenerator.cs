using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    [Generator]
    internal sealed class PolyEnumStructGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE_METADATA = $"{NAMESPACE}.PolyEnumStructAttribute";
        private const string ENUM_CASE_VALUE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseValueAttribute";
        private const string ENUM_CASE_IGNORE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseIgnoreAttribute";
        private const string UNDEFINED_NAME = "Undefined";
        private const string INTERFACE_NAME = "IEnumCase";
        private const string READ_ONLY_ATTRIBUTE = "global::System.ComponentModel.ReadOnlyAttribute";
        private const string STRUCT_LAYOUT_ATTRIBUTE = "global::System.Runtime.InteropServices.StructLayoutAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationInfo.GetCompilation(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  POLY_ENUM_STRUCT_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax { TypeParameterList: null }
                , ExtractSpec
            ).Where(static t => t.IsValid);

            var combined = candidateProvider
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

        private static PolyEnumStructSpec ExtractSpec(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax
                || context.TargetSymbol is not INamedTypeSymbol structSymbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var structSyntax = context.TargetNode;
            var attribute = context.Attributes[0];
            var semanticModel = context.SemanticModel;

            var assemblyName = semanticModel.Compilation.AssemblyName;
            var syntaxTree = structSyntax.SyntaxTree;
            var typeIdentifier = structSymbol.ToValidIdentifier();
            var hintName = syntaxTree.GetHintName(structSyntax, structSymbol.ToFileName());

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  structSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var isExplicitLayout = false;

            if (structSymbol.TryGetAttribute(STRUCT_LAYOUT_ATTRIBUTE, out var layoutAttrib))
            {
                foreach (var arg in layoutAttrib.ConstructorArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (arg.Value is int layoutKindValue
                        && Enum.IsDefined(typeof(LayoutKind), layoutKindValue)
                        && (LayoutKind)layoutKindValue == LayoutKind.Explicit
                    )
                    {
                        isExplicitLayout = true;
                        break;
                    }
                }
            }

            var result = new PolyEnumStructSpec {
                typeName = structSymbol.Name,
                typeNamespace = structSymbol.ContainingNamespace.ToDisplayString(),
                typeIdentifier = typeIdentifier,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                location = LocationInfo.From(structSyntax.GetLocation()),
                parentIsNamespace = structSyntax.Parent is BaseNamespaceDeclarationSyntax,
                isReadOnly = structSymbol.IsReadOnly,
                isExplicitLayout = isExplicitLayout,
            };

            foreach (var arg in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

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

            AggregateInterfaceAndStructs(ref result, structSymbol, token);

            return result;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationInfo compilation
            , PolyEnumStructSpec candidate
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
                var assemblyName = compilation.assemblyName;
                var hintName = candidate.hintName;
                var sourceFilePath = SourceGenHelpers.BuildSourceFilePath(assemblyName, hintName, projectPath);

                context.OutputSource(
                      outputSourceGenFiles
                    , candidate.openingSource
                    , candidate.WriteCode(compilation)
                    , candidate.closingSource
                    , candidate.hintName
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
                    , candidate.location.ToLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_POLY_ENUM_STRUCT_UNKNOWN_0001"
                , "Poly Enum Struct Generator Error"
                , "This error indicates a bug in the Poly Enum Struct source generators. Error message: '{0}'."
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

            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SC = global::System.Collections;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SD = global::System.Diagnostics;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETCol = global::EncosyTower.Collections;");
            p.PrintLine("using ETCon = global::EncosyTower.Conversion;");
            p.PrintLine("using ETEE = global::EncosyTower.EnumExtensions;");
            p.PrintLine("using ETEESG = global::EncosyTower.EnumExtensions.SourceGen;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintLine("using UC = global::Unity.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static void AggregateInterfaceAndStructs(
              ref PolyEnumStructSpec polyEnumStruct
            , INamedTypeSymbol structSymbol
            , CancellationToken token
        )
        {
            using var structsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.StructSpec>.Rent();

            var structName = structSymbol.Name;
            var verboseUndefinedName = $"{structName}_{UNDEFINED_NAME}";
            var paramList = new List<PolyEnumStructSpec.SlimParameterSpec>();

            PolyEnumStructSpec.StructSpec undefinedStruct = default;

            foreach (var memberType in structSymbol.GetTypeMembers())
            {
                token.ThrowIfCancellationRequested();

                if (memberType.IsUnboundGenericType)
                {
                    continue;
                }

                if (memberType.TypeKind == TypeKind.Struct)
                {
                    if (memberType.HasAttribute("global::EncosyTower.PolyEnumStructs.EnumCaseIgnore", token))
                    {
                        continue;
                    }

                    var @struct = GetStruct(memberType, paramList, token);

                    if (@struct.IsValid == false)
                    {
                        continue;
                    }

                    if (polyEnumStruct.definedUndefinedStruct == PolyEnumStructSpec.DefinedUndefinedStruct.None
                        && TryGetUndefinedCase(@struct.name, UNDEFINED_NAME, verboseUndefinedName, out var result)
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
                else if (memberType.TypeKind == TypeKind.Interface
                    && string.Equals(memberType.Name, INTERFACE_NAME, StringComparison.Ordinal)
                )
                {
                    polyEnumStruct.interfaceDef = GetInterface(memberType, token);
                }
            }

            if (undefinedStruct.IsValid)
            {
                structsBuilder.Add(undefinedStruct with {
                    isUndefined = true,
                    implicitlyDeclared = false,
                });
            }
            else if (polyEnumStruct.definedUndefinedStruct == PolyEnumStructSpec.DefinedUndefinedStruct.None)
            {
                structsBuilder.Add(new PolyEnumStructSpec.StructSpec {
                    name = verboseUndefinedName,
                    identifier = UNDEFINED_NAME,
                    isUndefined = true,
                    implicitlyDeclared = true,
                });
            }

            polyEnumStruct.structs = structsBuilder.ToImmutable();
            EnsureInterface(ref polyEnumStruct.interfaceDef);
        }

        private static bool TryGetUndefinedCase(
              string caseName
            , string undefinedName
            , string verboseUndefinedName
            , out PolyEnumStructSpec.DefinedUndefinedStruct result
        )
        {
            if (string.Equals(caseName, undefinedName, StringComparison.Ordinal))
            {
                result = PolyEnumStructSpec.DefinedUndefinedStruct.Default;
                return true;
            }

            if (string.Equals(caseName, verboseUndefinedName, StringComparison.Ordinal))
            {
                result = PolyEnumStructSpec.DefinedUndefinedStruct.Verbose;
                return true;
            }

            result = default;
            return false;
        }

        private static PolyEnumStructSpec.InterfaceSpec GetInterface(
              INamedTypeSymbol symbol
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var result = new PolyEnumStructSpec.InterfaceSpec {
                name = INTERFACE_NAME,
                definedInterface = true,
            };

            AggregateInterfaceMembers(symbol, ref result, token);

            return result;
        }

        private static void AggregateInterfaceMembers(
              INamedTypeSymbol symbol
            , ref PolyEnumStructSpec.InterfaceSpec interfaceDef
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration>.Rent();
            using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration>.Rent();
            using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration>.Rent();

            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

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

        private static void EnsureInterface(ref PolyEnumStructSpec.InterfaceSpec result)
        {
            if (result.IsValid)
            {
                return;
            }

            using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration>.Rent();
            using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration>.Rent();
            using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration>.Rent();

            result = new PolyEnumStructSpec.InterfaceSpec {
                name = INTERFACE_NAME,
                properties = propertiesBuilder.ToImmutable(),
                indexers = indexersBuilder.ToImmutable(),
                methods = methodsBuilder.ToImmutable(),
                definedInterface = false,
            };
        }

        private static PolyEnumStructSpec.StructSpec GetStruct(
              INamedTypeSymbol symbol
            , List<PolyEnumStructSpec.SlimParameterSpec> paramList
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (symbol.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token))
            {
                return default;
            }

            var result = new PolyEnumStructSpec.StructSpec {
                name = symbol.Name,
                displayName = symbol.GetDisplayNameOrDefault(symbol.Name, token),
                identifier = symbol.ToSimpleValidIdentifier(),
                isUndefined = false,
                isReadOnly = symbol.IsReadOnly,
                isRecord = symbol.IsRecord,
            };

            AggregateConstructions(ref result, symbol, token);
            AggregatePrimaryParameters(ref result, symbol, paramList, token);
            AggregateStructMembers(ref result, symbol, token);

            return result;
        }

        private static void AggregateConstructions(
              ref PolyEnumStructSpec.StructSpec structDef
            , ITypeSymbol symbol
            , CancellationToken token
        )
        {
            using var arrayBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.ConstructionSpec>.Rent();

            foreach (var attribute in symbol.GetAttributes(ENUM_CASE_VALUE_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length < 1)
                {
                    continue;
                }

                var arg = attribute.ConstructorArguments[0];

                if (arg.IsNull == false
                    && arg.Kind is TypedConstantKind.Primitive or TypedConstantKind.Enum
                    && arg.Type is INamedTypeSymbol
                )
                {
                    arrayBuilder.Add(new PolyEnumStructSpec.ConstructionSpec {
                        type = GetType(arg.Type),
                        value = GetCreationValue(arg.Type, arg.Value, token),
                    });
                }
            }

            structDef.constructions = arrayBuilder.ToImmutable();
        }

        private static void AggregatePrimaryParameters(
              ref PolyEnumStructSpec.StructSpec structDef
            , INamedTypeSymbol symbol
            , List<PolyEnumStructSpec.SlimParameterSpec> paramList
            , CancellationToken token
        )
        {
            paramList.Clear();

            var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.ParameterSpec>.Rent();

            foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                var node = syntaxRef.GetSyntax(token);

                if (node is RecordDeclarationSyntax recordSyntax)
                {
                    if (recordSyntax.ParameterList is { } paramListSyntax)
                    {
                        AggregateParamList(paramListSyntax, paramList, token);
                        FillParametersBuilder(ref parametersBuilder, symbol, paramList, token);
                        break;
                    }
                }
            }

            structDef.parameters = parametersBuilder.ToImmutable();

            return;

            static void AggregateParamList(
                  ParameterListSyntax paramListSyntax
                , List<PolyEnumStructSpec.SlimParameterSpec> paramList
                , CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();

                var parameters = paramListSyntax.Parameters;
                var paramCount = parameters.Count;

                if (paramList.Capacity < paramCount)
                {
                    paramList.Capacity = paramCount;
                }

                for (var i = 0; i < paramCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var parameter = parameters[i];
                    string typeName;

                    if (parameter.Type is PredefinedTypeSyntax preType)
                    {
                        typeName = preType.Keyword.ValueText;
                    }
                    else if (parameter.Type is IdentifierNameSyntax idType)
                    {
                        typeName = idType.Identifier.Text;
                    }
                    else if (parameter.Type is QualifiedNameSyntax qType)
                    {
                        typeName = qType.ToString();
                    }
                    else
                    {
                        continue;
                    }

                    var paramDef = new PolyEnumStructSpec.SlimParameterSpec {
                        name = parameter.Identifier.Text,
                        type = new PolyEnumStructSpec.TypeSpec {
                            name = typeName,
                            identifier = typeName.ToValidIdentifier(),
                        },
                    };

                    var modifiers = parameter.Modifiers;
                    var modCount = modifiers.Count;

                    for (var k = 0; k < modCount; k++)
                    {
                        token.ThrowIfCancellationRequested();

                        var mod = modifiers[k];

                        if (mod.IsKind(SyntaxKind.RefKeyword))
                        {
                            paramDef.refKind = RefKind.Ref;
                            break;
                        }

                        if (mod.IsKind(SyntaxKind.InKeyword))
                        {
                            paramDef.refKind = RefKind.In;
                            break;
                        }
                    }

                    paramList.Add(paramDef);
                }
            }

            static void FillParametersBuilder(
                  ref ImmutableArrayBuilder<PolyEnumStructSpec.ParameterSpec> parametersBuilder
                , INamedTypeSymbol symbol
                , List<PolyEnumStructSpec.SlimParameterSpec> paramList
                , CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();

                if (paramList.Count < 1)
                {
                    return;
                }

                var length = paramList.Count;
                var isReadOnly = symbol.IsReadOnly;

                foreach (var constructor in symbol.Constructors)
                {
                    token.ThrowIfCancellationRequested();

                    if (constructor.Parameters.Length != length)
                    {
                        continue;
                    }

                    for (var i = 0; i < length; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        var ctorParam = constructor.Parameters[i];
                        var paramDef = paramList[i];

                        if (ctorParam.RefKind != paramDef.refKind)
                        {
                            break;
                        }

                        if (ctorParam.Name != paramDef.name)
                        {
                            break;
                        }

                        var fullName = ctorParam.Type.ToFullName();

                        if (fullName.EndsWith(paramDef.type.name) == false)
                        {
                            continue;
                        }

                        int fieldSize = 0;
                        ctorParam.Type.GetUnmanagedSize(ref fieldSize, token);

                        parametersBuilder.Add(new PolyEnumStructSpec.ParameterSpec {
                            refKind = paramDef.refKind,
                            field = new PolyEnumStructSpec.FieldSpec {
                                name = paramDef.name,
                                returnType = GetType(ctorParam.Type),
                                size = fieldSize,
                                implicityDeclared = true,
                                isReadOnly = isReadOnly,
                            },
                        });
                    }
                }
            }
        }

        private static void AggregateStructMembers(
              ref PolyEnumStructSpec.StructSpec structDef
            , ITypeSymbol symbol
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var fieldsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.FieldSpec>.Rent();
            using var propertiesBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration>.Rent();
            using var indexersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration>.Rent();
            using var methodsBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration>.Rent();

            int structSize = 0;
            var isReadOnly = symbol.IsReadOnly;

            foreach (var member in symbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

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
                    fieldSymbol.GetUnmanagedSize(ref fieldSize, token);
                    structSize += fieldSize;

                    fieldsBuilder.Add(new PolyEnumStructSpec.FieldSpec {
                        name = fieldName,
                        returnType = GetType(fieldSymbol.Type),
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
                    GetStructMember(member, isReadOnly, token, propertiesBuilder, indexersBuilder, methodsBuilder);
                }
            }

            structDef.fields = fieldsBuilder.ToImmutable();
            structDef.properties = propertiesBuilder.ToImmutable();
            structDef.indexers = indexersBuilder.ToImmutable();
            structDef.methods = methodsBuilder.ToImmutable();
            structDef.size = structSize;
        }

        private static bool TryGetInterfaceMember(
              ISymbol member
            , ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration> propertiesBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration> indexersBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration> methodsBuilder
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
                            token.ThrowIfCancellationRequested();

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

                if (propertySymbol.TryGetAttribute(READ_ONLY_ATTRIBUTE, out var attrib, token)
                    && attrib.ConstructorArguments.Length == 1
                )
                {
                    isReadOnly = (bool)attrib.ConstructorArguments[0].Value;
                }

                if (propertySymbol.IsIndexer)
                {
                    var indexerDef = new PolyEnumStructSpec.IndexerDeclaration {
                        returnType = GetType(propertySymbol.Type),
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, getterIsDim),
                        setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, setterIsDim),
                        refKind = propertySymbol.RefKind,
                    };

                    using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                    foreach (var parameter in propertySymbol.Parameters)
                    {
                        parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                            name = parameter.Name,
                            type = GetType(parameter.Type),
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
                    var propertyDef = new PolyEnumStructSpec.PropertyDeclaration {
                        name = propertySymbol.Name,
                        returnType = GetType(propertySymbol.Type),
                        refKind = propertySymbol.RefKind,
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, getterIsDim),
                        setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, setterIsDim),
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

                var methodDef = new PolyEnumStructSpec.MethodDeclaration {
                    name = methodSymbol.Name,
                    returnType = GetType(methodSymbol.ReturnType),
                    refKind = methodSymbol.RefKind,
                    returnsVoid = methodSymbol.ReturnsVoid,
                    isReadOnly = methodSymbol.IsReadOnly,
                    isDim = isDim,
                };

                using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                foreach (var parameter in methodSymbol.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                        name = parameter.Name,
                        type = GetType(parameter.Type),
                        refKind = parameter.RefKind,
                    });
                }

                methodDef.parameters = parametersBuilder.ToImmutable();
                methodsBuilder.Add(methodDef);
            }

            return true;
        }

        private static void GetStructMember(
              ISymbol member
            , bool isReadOnly
            , CancellationToken token
            , ImmutableArrayBuilder<PolyEnumStructSpec.PropertyDeclaration> propertiesBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.IndexerDeclaration> indexersBuilder
            , ImmutableArrayBuilder<PolyEnumStructSpec.MethodDeclaration> methodsBuilder
        )
        {
            token.ThrowIfCancellationRequested();

            if (member is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.ExplicitInterfaceImplementations.Length > 0)
                {
                    return;
                }

                if (propertySymbol.IsIndexer)
                {
                    var indexerDecl = new PolyEnumStructSpec.IndexerDeclaration {
                        returnType = GetType(propertySymbol.Type),
                        refKind = propertySymbol.RefKind,
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, false),
                    };

                    if (isReadOnly == false && propertySymbol.IsReadOnly == false)
                    {
                        indexerDecl.setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, false);
                    }

                    using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                    foreach (var parameter in propertySymbol.Parameters)
                    {
                        token.ThrowIfCancellationRequested();

                        parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                            name = parameter.Name,
                            type = GetType(parameter.Type),
                            refKind = parameter.RefKind,
                        });
                    }

                    indexerDecl.parameters = parametersBuilder.ToImmutable();
                    indexersBuilder.Add(indexerDecl);
                }
                else
                {
                    var propDecl = new PolyEnumStructSpec.PropertyDeclaration {
                        name = propertySymbol.Name,
                        returnType = GetType(propertySymbol.Type),
                        refKind = propertySymbol.RefKind,
                        getter = GetPropertyMethod(propertySymbol.GetMethod, token, isGetter: true, false),
                    };

                    if (isReadOnly == false && propertySymbol.IsReadOnly == false)
                    {
                        propDecl.setter = GetPropertyMethod(propertySymbol.SetMethod, token, isGetter: false, false);
                    }

                    propertiesBuilder.Add(propDecl);
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

                var methodDef = new PolyEnumStructSpec.MethodDeclaration {
                    name = methodSymbol.Name,
                    returnType = GetType(methodSymbol.ReturnType),
                    refKind = methodSymbol.RefKind,
                    returnsVoid = methodSymbol.ReturnsVoid,
                    isReadOnly = methodSymbol.IsReadOnly,
                };

                using var parametersBuilder = ImmutableArrayBuilder<PolyEnumStructSpec.SlimParameterSpec>.Rent();

                foreach (var parameter in methodSymbol.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    parametersBuilder.Add(new PolyEnumStructSpec.SlimParameterSpec {
                        name = parameter.Name,
                        type = GetType(parameter.Type),
                        refKind = parameter.RefKind,
                    });
                }

                methodDef.parameters = parametersBuilder.ToImmutable();
                methodsBuilder.Add(methodDef);
            }
        }

        private static PolyEnumStructSpec.TypeSpec GetType(ITypeSymbol typeSymbol)
        {
            return new PolyEnumStructSpec.TypeSpec {
                name = typeSymbol.ToFullName(),
                identifier = typeSymbol.ToValidIdentifier(),
                isEnum = typeSymbol.IsEnumType(),
            };
        }

        private static string GetCreationValue(ITypeSymbol typeSymbol, object value, CancellationToken token)
        {
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                return typeSymbol.GetEnumMemberName(value, token);
            }

            if (typeSymbol.SpecialType == SpecialType.System_String)
            {
                return $"\"{value}\"";
            }

            return value.ToString();
        }

        private static PolyEnumStructSpec.PropertyMethodDeclaration GetPropertyMethod(
              IMethodSymbol methodSymbol
            , CancellationToken token
            , bool isGetter
            , bool isDim
        )
        {
            if (methodSymbol is null)
            {
                return default;
            }

            var isReadOnly = methodSymbol.IsReadOnly;

            if (methodSymbol.TryGetAttribute(READ_ONLY_ATTRIBUTE, out var attrib, token)
                && attrib.ConstructorArguments.Length == 1
            )
            {
                isReadOnly = (bool)attrib.ConstructorArguments[0].Value;
            }

            return new PolyEnumStructSpec.PropertyMethodDeclaration {
                isValid = true,
                refKind = methodSymbol.RefKind,
                isGetter = isGetter,
                isReadOnly = isReadOnly,
                isDim = isDim,
            };
        }
    }
}
