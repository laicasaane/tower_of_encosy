using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Generators.Mvvm.Binders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    public partial struct MonoBinderSpec : IEquatable<MonoBinderSpec>
    {
        private const string MONO_BINDER_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBinderAttribute";
        private const string MONO_BINDING_PROP_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBindingPropertyAttribute";
        private const string MONO_BINDING_CMD_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBindingCommandAttribute";
        private const string MONO_BINDING_EXCLUDE_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBindingExcludeAttribute";
        private const string MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBinderExcludeParentAttribute";
        private const string UNITY_EVENT_BASE_TYPE = "global::UnityEngine.Events.UnityEventBase";
        private const string UNITY_OBJECT_TYPE = "global::UnityEngine.Object";

        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string hintName;
        public string userClassName;
        public string userNamespace;
        public string componentFullTypeName;
        public string componentLabelName;
        public string preprocessorGuard;
        public EquatableArray<PropertyBindingSpec> propertyBindings;
        public EquatableArray<CommandBindingSpec> commandBindings;
        public bool isOuterClassSealed;
        public string outerTypeIdentifier;
        public bool hasOnBindPropertyFailedMethod;
        public bool hasOnBindCommandFailedMethod;
        public EquatableArray<BinderSpec.BindingPropertySpec> userBindingPropertyRefs;
        public EquatableArray<BinderSpec.BindingCommandSpec> userBindingCommandRefs;
        public EquatableArray<BinderSpec.NonVariantTypeSpec> outerNonVariantTypes;

        public readonly bool IsValid
            => string.IsNullOrEmpty(userClassName) == false
            && string.IsNullOrEmpty(componentFullTypeName) == false;

        public static MonoBinderSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax
                || context.TargetSymbol is not INamedTypeSymbol classSymbol
            )
            {
                return default;
            }

            var monoBinderAttr = classSymbol.GetAttribute(MONO_BINDER_ATTRIBUTE, token);

            if (monoBinderAttr is null
                || monoBinderAttr.ConstructorArguments.Length < 1
                || monoBinderAttr.ConstructorArguments[0].Value is not INamedTypeSymbol componentType
            )
            {
                return default;
            }

            var preprocessorGuard = string.Empty;
            var excludeObsolete = false;

            foreach (var namedArg in monoBinderAttr.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(namedArg.Key, "PreprocessorGuard", StringComparison.Ordinal))
                {
                    preprocessorGuard = namedArg.Value.Value as string ?? string.Empty;
                }
                else if (string.Equals(namedArg.Key, "ExcludeObsolete", StringComparison.Ordinal)
                    && namedArg.Value.Value is bool boolVal)
                {
                    excludeObsolete = boolVal;
                }
            }

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var userClassName = classSyntax.Identifier.Text;
            var userNamespace = classSymbol.ContainingNamespace is { IsGlobalNamespace: false } ns
                ? ns.ToDisplayString()
                : string.Empty;

            var componentFullTypeName = componentType.ToFullName();
            var componentLabelName = MemberNameToLabel(componentType.Name, token);
            var compilation = context.SemanticModel.Compilation;
            var excludedMembers = new HashSet<string>(StringComparer.Ordinal);

            foreach (var attrData in classSymbol.GetAttributes(MONO_BINDING_EXCLUDE_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                if (attrData.ConstructorArguments.Length < 1
                    || attrData.ConstructorArguments[0].Value is not string excludedName
                    || string.IsNullOrEmpty(excludedName)
                )
                {
                    continue;
                }

                excludedMembers.Add(excludedName);
            }

            INamedTypeSymbol excludedParentType = null;
            {
                var excludeParentAttr = classSymbol.GetAttribute(MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE, token);

                if (excludeParentAttr != null
                    && excludeParentAttr.ConstructorArguments.Length >= 1
                    && excludeParentAttr.ConstructorArguments[0].Value is INamedTypeSymbol excludedParent
                )
                {
                    excludedParentType = excludedParent;
                }
            }

            var explicitProps = new Dictionary<string, (bool useCustomSetter, string label)>(StringComparer.Ordinal);
            var explicitCmds = new Dictionary<string, (string wrapperTypeName, string label)>(StringComparer.Ordinal);

            foreach (var attrData in classSymbol.GetAttributes(MONO_BINDING_PROP_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                if (attrData.ConstructorArguments.Length < 1
                    || attrData.ConstructorArguments[0].Value is not string memberName
                    || string.IsNullOrEmpty(memberName)
                )
                {
                    continue;
                }

                token.ThrowIfCancellationRequested();

                var useCustomSetter = false;
                var labelOverride = string.Empty;

                foreach (var namedArg in attrData.NamedArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (string.Equals(namedArg.Key, "UseCustomSetter", StringComparison.Ordinal)
                        && namedArg.Value.Value is bool useCustomSetterVal)
                    {
                        useCustomSetter = useCustomSetterVal;
                    }
                    else if (string.Equals(namedArg.Key, "Label", StringComparison.Ordinal))
                    {
                        labelOverride = namedArg.Value.Value as string ?? string.Empty;
                    }
                }

                explicitProps[memberName] = (useCustomSetter, labelOverride);
            }

            foreach (var attrData in classSymbol.GetAttributes(MONO_BINDING_CMD_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                if (attrData.ConstructorArguments.Length < 1
                    || attrData.ConstructorArguments[0].Value is not string memberName
                    || string.IsNullOrEmpty(memberName)
                )
                {
                    continue;
                }

                token.ThrowIfCancellationRequested();

                var wrapperTypeName = string.Empty;
                var labelOverride = string.Empty;

                foreach (var namedArg in attrData.NamedArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (string.Equals(namedArg.Key, "WrapperType", StringComparison.Ordinal))
                    {
                        if (namedArg.Value.Value is INamedTypeSymbol wrapperType)
                        {
                            wrapperTypeName = wrapperType.ToFullName();
                        }
                    }
                    else if (string.Equals(namedArg.Key, "Label", StringComparison.Ordinal))
                    {
                        labelOverride = namedArg.Value.Value as string ?? string.Empty;
                    }
                }

                explicitCmds[memberName] = (wrapperTypeName, labelOverride);
            }

            using var propBuilder = ImmutableArrayBuilder<PropertyBindingSpec>.Rent();
            using var cmdBuilder = ImmutableArrayBuilder<CommandBindingSpec>.Rent();
            var seenPropMembers = new HashSet<string>(StringComparer.Ordinal);
            var seenCmdMembers = new HashSet<string>(StringComparer.Ordinal);
            var current = componentType as ITypeSymbol;

            while (current != null && current.HasFullName(UNITY_OBJECT_TYPE, token) == false)
            {
                token.ThrowIfCancellationRequested();

                if (excludedParentType != null
                    && SymbolEqualityComparer.Default.Equals(current, excludedParentType)
                )
                {
                    break;
                }

                var members = current.GetMembers();

                foreach (var member in members)
                {
                    token.ThrowIfCancellationRequested();

                    if (member.DeclaredAccessibility != Accessibility.Public || member.IsStatic)
                    {
                        continue;
                    }

                    if (member is IPropertySymbol prop)
                    {
                        var tryProperty = TryCollectProperty(
                              prop
                            , token
                            , userClassName
                            , userNamespace
                            , explicitProps
                            , seenPropMembers
                            , excludedMembers
                            , excludeObsolete
                            , compilation
                            , propBuilder
                        );

                        if (tryProperty == false)
                        {
                            TryCollectCommand(
                                  token
                                , prop.Name
                                , prop.Type
                                , userClassName
                                , userNamespace
                                , explicitCmds
                                , seenCmdMembers
                                , excludedMembers
                                , excludeObsolete
                                , compilation
                                , cmdBuilder
                                , memberSymbol: prop
                            );
                        }

                        continue;
                    }

                    if (member is IFieldSymbol field)
                    {
                        TryCollectCommand(
                              token
                            , field.Name
                            , field.Type
                            , userClassName
                            , userNamespace
                            , explicitCmds
                            , seenCmdMembers
                            , excludedMembers
                            , excludeObsolete
                            , compilation
                            , cmdBuilder
                            , memberSymbol: field
                        );
                        continue;
                    }

                    if (member is IEventSymbol evt)
                    {
                        TryCollectCommand(
                              token
                            , evt.Name
                            , evt.Type
                            , userClassName
                            , userNamespace
                            , explicitCmds
                            , seenCmdMembers
                            , excludedMembers
                            , excludeObsolete
                            , compilation
                            , cmdBuilder
                            , memberSymbol: evt
                        );
                    }
                }

                current = current.BaseType;
            }

            token.ThrowIfCancellationRequested();

            foreach (var kvp in explicitCmds)
            {
                token.ThrowIfCancellationRequested();

                if (seenCmdMembers.Contains(kvp.Key))
                {
                    continue;
                }

                if (excludedMembers.Contains(kvp.Key))
                {
                    continue;
                }

                var memberSymbol = FindMember(componentType, kvp.Key, token);

                if (memberSymbol == null)
                {
                    continue;
                }

                ITypeSymbol memberType = memberSymbol switch {
                    IFieldSymbol fs => fs.Type,
                    IPropertySymbol ps => ps.Type,
                    IEventSymbol es => es.Type,
                    _ => null,
                };

                if (memberType == null)
                {
                    continue;
                }

                TryCollectCommand(
                      token
                    , kvp.Key
                    , memberType
                    , userClassName
                    , userNamespace
                    , explicitCmds
                    , seenCmdMembers
                    , excludedMembers
                    , excludeObsolete
                    , compilation
                    , cmdBuilder
                    , memberSymbol: memberSymbol
                    , forceExplicit: true
                );
            }

            token.ThrowIfCancellationRequested();

            foreach (var kvp in explicitProps)
            {
                token.ThrowIfCancellationRequested();

                if (seenPropMembers.Contains(kvp.Key))
                {
                    continue;
                }

                var memberSymbol = FindMember(componentType, kvp.Key, token);

                if (memberSymbol is IPropertySymbol propSymbol)
                {
                    TryCollectProperty(
                          propSymbol
                        , token
                        , userClassName
                        , userNamespace
                        , explicitProps
                        , seenPropMembers
                        , excludedMembers
                        , excludeObsolete
                        , compilation
                        , propBuilder
                    );
                }
                else
                {
                    var methodSymbol = FindOneParamVoidMethod(componentType, kvp.Key, token);

                    if (methodSymbol != null)
                    {
                        TryCollectPropertyFromMethod(
                              methodSymbol
                            , token
                            , kvp.Key
                            , kvp.Value
                            , userClassName
                            , userNamespace
                            , seenPropMembers
                            , compilation
                            , propBuilder
                        );
                    }
                }
            }

            var monoBinderBaseSymbol = compilation.GetTypeByMetadataName("EncosyTower.Mvvm.ViewBinding.Components.MonoBinder`1");
            var monoBindingPropBaseSymbol = compilation.GetTypeByMetadataName("EncosyTower.Mvvm.ViewBinding.Components.MonoBindingProperty`1");
            var monoBindingCmdBaseSymbol = compilation.GetTypeByMetadataName("EncosyTower.Mvvm.ViewBinding.Components.MonoBindingCommand`1");
            var isOuterClassSealed = classSymbol.IsSealed;
            var outerTypeIdentifier = classSymbol.ToValidIdentifier();
            var hasOnBindPropertyFailedMethod = false;
            var hasOnBindCommandFailedMethod = false;

            token.ThrowIfCancellationRequested();

            {
                var walkType = classSymbol;
                var isCurrentType = true;

                while (walkType != null)
                {
                    token.ThrowIfCancellationRequested();

                    foreach (var member in walkType.GetMembers("OnBindPropertyFailed"))
                    {
                        token.ThrowIfCancellationRequested();

                        if (member is IMethodSymbol checkMethod
                            && isCurrentType == false
                            && checkMethod.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
                            && checkMethod.Parameters.Length == 1
                            && checkMethod.Parameters[0].Type.HasFullName(BinderSpec.BINDING_PROPERTY, token)
                        )
                        {
                            hasOnBindPropertyFailedMethod = true;
                            break;
                        }
                    }

                    foreach (var member in walkType.GetMembers("OnBindCommandFailed"))
                    {
                        token.ThrowIfCancellationRequested();

                        if (member is IMethodSymbol checkMethod
                            && isCurrentType == false
                            && checkMethod.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
                            && checkMethod.Parameters.Length == 1
                            && checkMethod.Parameters[0].Type.HasFullName(BinderSpec.BINDING_COMMAND, token)
                        )
                        {
                            hasOnBindCommandFailedMethod = true;
                            break;
                        }
                    }

                    isCurrentType = false;
                    walkType = walkType.BaseType;
                }
            }

            token.ThrowIfCancellationRequested();

            var outerBindingPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            var outerConverterNames = new HashSet<string>(StringComparer.Ordinal);
            var outerBindingCommandNames = new HashSet<string>(StringComparer.Ordinal);
            var outerNonVariantTypeFilter = new Dictionary<string, BinderSpec.NonVariantTypeSpec>(StringComparer.Ordinal);
            var semanticModel = context.SemanticModel;

            using var tempOuterPropMethods = ImmutableArrayBuilder<OuterBinderPropertyScanEntry>.Rent();
            using var tempOuterCmdMethods = ImmutableArrayBuilder<OuterBinderCommandScanEntry>.Rent();

            foreach (var outerMember in classSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (outerMember is IMethodSymbol outerMethod)
                {
                    var bindingPropAttr = outerMethod.GetAttribute(BinderSpec.BINDING_PROPERTY_ATTRIBUTE, token);
                    var bindingCmdAttr = outerMethod.GetAttribute(BinderSpec.BINDING_COMMAND_ATTRIBUTE, token);

                    if (bindingPropAttr != null && outerMethod.Parameters.Length < 2)
                    {
                        var outerParamType = string.Empty;
                        var outerVarConvPropName = string.Empty;
                        var outerParamRefKind = RefKind.None;
                        var outerIsParamVariant = true;

                        if (outerMethod.Parameters.Length == 1)
                        {
                            var outerParam = outerMethod.Parameters[0];

                            if (outerParam.RefKind is not (RefKind.None or RefKind.In or RefKind.Ref))
                            {
                                continue;
                            }

                            outerParamRefKind = outerParam.RefKind;
                            outerParamType = outerParam.Type.ToFullName();
                            outerIsParamVariant = outerParamType == BinderSpec.VARIANT_TYPE;

                            if (outerIsParamVariant == false)
                            {
                                outerVarConvPropName = outerParam.Type
                                    .ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();
                            }
                        }

                        tempOuterPropMethods.Add(new OuterBinderPropertyScanEntry {
                            method = outerMethod,
                            paramFullTypeName = outerParamType,
                            paramRefKind = outerParamRefKind,
                            variantConverterPropName = outerVarConvPropName,
                            isParameterTypeVariant = outerIsParamVariant,
                        });

                        if (outerIsParamVariant == false
                            && string.IsNullOrEmpty(outerParamType) == false
                            && outerNonVariantTypeFilter.ContainsKey(outerParamType) == false
                        )
                        {
                            outerNonVariantTypeFilter[outerParamType] = new BinderSpec.NonVariantTypeSpec {
                                fullTypeName = outerParamType,
                                converterPropertyName = outerVarConvPropName,
                            };
                        }
                    }
                    else if (bindingCmdAttr != null
                        && outerMethod.IsPartialDefinition
                        && outerMethod.ReturnsVoid
                        && outerMethod.Parameters.Length < 2
                    )
                    {
                        var outerCmdParamType = string.Empty;
                        var outerCmdParamRefKind = RefKind.None;
                        var outerCmdParamName = string.Empty;

                        if (outerMethod.Parameters.Length == 1
                            && outerMethod.Parameters[0].RefKind is RefKind.None or RefKind.In or RefKind.Ref
                        )
                        {
                            var outerParam = outerMethod.Parameters[0];
                            outerCmdParamType = outerParam.Type.ToFullName();
                            outerCmdParamRefKind = outerParam.RefKind;
                            outerCmdParamName = outerParam.Name;
                        }

                        tempOuterCmdMethods.Add(new OuterBinderCommandScanEntry {
                            method = outerMethod,
                            paramFullTypeName = outerCmdParamType,
                            paramRefKind = outerCmdParamRefKind,
                            paramName = outerCmdParamName,
                        });
                    }

                    continue;
                }

                if (outerMember is IFieldSymbol outerField)
                {
                    var outerFieldTypeName = outerField.Type.ToFullName();

                    if (outerFieldTypeName == BinderSpec.BINDING_PROPERTY)
                    {
                        outerBindingPropertyNames.Add(outerField.Name);
                    }
                    else if (outerFieldTypeName == BinderSpec.CONVERTER)
                    {
                        outerConverterNames.Add(outerField.Name);
                    }
                    else if (outerFieldTypeName == BinderSpec.BINDING_COMMAND)
                    {
                        outerBindingCommandNames.Add(outerField.Name);
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            using var outerPropRefBuilder = ImmutableArrayBuilder<BinderSpec.BindingPropertySpec>.Rent();
            using var outerCmdRefBuilder = ImmutableArrayBuilder<BinderSpec.BindingCommandSpec>.Rent();

            foreach (var entry in tempOuterPropMethods.WrittenSpan)
            {
                token.ThrowIfCancellationRequested();

                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , out var propForwardedFieldAttrs
                    , out _
                );

                outerPropRefBuilder.Add(new BinderSpec.BindingPropertySpec {
                    methodName = entry.method.Name,
                    paramFullTypeName = entry.paramFullTypeName,
                    paramRefKind = entry.paramRefKind,
                    variantConverterPropertyName = entry.variantConverterPropName,
                    isParameterTypeVariant = entry.isParameterTypeVariant,
                    skipBindingProperty = outerBindingPropertyNames.Contains($"_bindingFieldFor{entry.method.Name}"),
                    skipConverter = outerConverterNames.Contains($"_converterFor{entry.method.Name}"),
                    forwardedFieldAttributes = propForwardedFieldAttrs.AsEquatableArray(),
                });
            }

            token.ThrowIfCancellationRequested();

            foreach (var entry in tempOuterCmdMethods.WrittenSpan)
            {
                token.ThrowIfCancellationRequested();

                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , false
                    , out var cmdForwardedFieldAttrs
                    , out _
                );

                outerCmdRefBuilder.Add(new BinderSpec.BindingCommandSpec {
                    methodName = entry.method.Name,
                    paramFullTypeName = entry.paramFullTypeName,
                    paramRefKind = entry.paramRefKind,
                    paramName = entry.paramName,
                    skipBindingCommand = outerBindingCommandNames.Contains($"_bindingCommandFor{entry.method.Name}"),
                    forwardedFieldAttributes = cmdForwardedFieldAttrs.AsEquatableArray(),
                });
            }

            token.ThrowIfCancellationRequested();

            using var outerNonVariantTypesBuilder = ImmutableArrayBuilder<BinderSpec.NonVariantTypeSpec>.Rent();

            foreach (var nvt in outerNonVariantTypeFilter.Values)
            {
                token.ThrowIfCancellationRequested();

                outerNonVariantTypesBuilder.Add(nvt);
            }

            var syntaxTree = classSyntax.SyntaxTree;
            var hintName = syntaxTree.GetHintName( classSyntax, classSymbol.ToFileName());

            return new MonoBinderSpec {
                location = LocationInfo.From(classSyntax.GetLocation()),
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                userClassName = userClassName,
                userNamespace = userNamespace,
                componentFullTypeName = componentFullTypeName,
                componentLabelName = componentLabelName,
                preprocessorGuard = preprocessorGuard,
                propertyBindings = propBuilder.ToImmutable().AsEquatableArray(),
                commandBindings = cmdBuilder.ToImmutable().AsEquatableArray(),
                isOuterClassSealed = isOuterClassSealed,
                outerTypeIdentifier = outerTypeIdentifier,
                hasOnBindPropertyFailedMethod = hasOnBindPropertyFailedMethod,
                hasOnBindCommandFailedMethod = hasOnBindCommandFailedMethod,
                userBindingPropertyRefs = outerPropRefBuilder.ToImmutable().AsEquatableArray(),
                userBindingCommandRefs = outerCmdRefBuilder.ToImmutable().AsEquatableArray(),
                outerNonVariantTypes = outerNonVariantTypesBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static bool TryCollectProperty(
              IPropertySymbol prop
            , CancellationToken token
            , string userClassName
            , string userNamespace
            , Dictionary<string, (bool useCustomSetter, string label)> explicitProps
            , HashSet<string> seen
            , HashSet<string> excludedMembers
            , bool excludeObsolete
            , Compilation compilation
            , ImmutableArrayBuilder<PropertyBindingSpec> builder
        )
        {
            if (excludedMembers.Contains(prop.Name))
            {
                return false;
            }

            if (prop.IsIndexer || prop.SetMethod == null)
            {
                return false;
            }

            if (prop.SetMethod.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            var isUnityEvent = prop.Type.InheritsFromType(UNITY_EVENT_BASE_TYPE);
            var isDelegate = isUnityEvent == false && prop.Type.TypeKind == TypeKind.Delegate;

            if (isUnityEvent || isDelegate)
            {
                return false;
            }

            var memberName = prop.Name;

            if (seen.Add(memberName) == false)
            {
                return false;
            }

            var isExplicit = explicitProps.ContainsKey(memberName);
            var (isObsolete, obsoleteMessage) = GetObsoleteInfo(prop, token);

            if (isObsolete && excludeObsolete && !isExplicit)
            {
                return false;
            }

            explicitProps.TryGetValue(memberName, out var explicitInfo);

            var label = string.IsNullOrEmpty(explicitInfo.label)
                ? MemberNameToLabel(memberName, token)
                : explicitInfo.label;

            var memberPascalName = MakeFirstCharUpper(memberName);
            var generatedClassName = $"Binding{memberPascalName}";
            var setterMethodName = $"Set{memberPascalName}";
            var propFullTypeName = prop.Type.ToFullName();
            var needsIn = NeedsInModifier(prop.Type, token);

            var variantConverterPropertyName = propFullTypeName == BinderSpec.VARIANT_TYPE
                ? string.Empty
                : prop.Type.ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();

            var qualifiedName = string.IsNullOrEmpty(userNamespace)
                ? $"{userClassName}+{generatedClassName}"
                : $"{userNamespace}.{userClassName}+{generatedClassName}";

            var skip = compilation.GetTypeByMetadataName(qualifiedName) != null;

            builder.Add(new PropertyBindingSpec {
                memberName = memberName,
                memberPascalName = memberPascalName,
                propFullTypeName = propFullTypeName,
                needsInModifier = needsIn,
                setterMethod = string.Empty,
                label = label,
                setterMethodName = setterMethodName,
                generatedClassName = generatedClassName,
                skipGeneration = skip,
                variantConverterPropertyName = variantConverterPropertyName,
                isObsolete = isObsolete && !isExplicit,
                obsoleteMessage = isObsolete && !isExplicit ? obsoleteMessage : string.Empty,
                useCustomSetter = explicitInfo.useCustomSetter,
                customSetterPartialMethodName = explicitInfo.useCustomSetter
                    ? DeriveCustomSetterPartialName(memberName)
                    : string.Empty,
            });

            return true;
        }

        private static bool TryCollectCommand(
              CancellationToken token
            , string memberName
            , ITypeSymbol memberType
            , string userClassName
            , string userNamespace
            , Dictionary<string, (string wrapperTypeName, string label)> explicitCmds
            , HashSet<string> seen
            , HashSet<string> excludedMembers
            , bool excludeObsolete
            , Compilation compilation
            , ImmutableArrayBuilder<CommandBindingSpec> builder
            , ISymbol memberSymbol = null
            , bool forceExplicit = false
        )
        {
            if (excludedMembers.Contains(memberName))
            {
                return false;
            }

            var isUnityEvent = memberType.InheritsFromType(UNITY_EVENT_BASE_TYPE);
            var isDelegate = isUnityEvent == false && memberType.TypeKind == TypeKind.Delegate;

            if (isUnityEvent == false && isDelegate == false)
            {
                return false;
            }

            if (seen.Add(memberName) == false)
            {
                return false;
            }

            var isExplicit = explicitCmds.ContainsKey(memberName);
            var (isObsolete, obsoleteMessage) = memberSymbol != null
                ? GetObsoleteInfo(memberSymbol, token)
                : (false, string.Empty);

            if (isObsolete && excludeObsolete && !isExplicit)
            {
                return false;
            }

            ImmutableArray<ITypeSymbol> typeArgs;

            if (isUnityEvent)
            {
                var unityEventBase = FindUnityEventBase(memberType, token);
                typeArgs = unityEventBase?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
            }
            else
            {
                typeArgs = GetDelegateInvokeParameters(memberType, token);
            }

            explicitCmds.TryGetValue(memberName, out var explicitInfo);

            if (forceExplicit == false
                && typeArgs.Length > 1
                && string.IsNullOrEmpty(explicitInfo.wrapperTypeName)
            )
            {
                return false;
            }

            var label = string.IsNullOrEmpty(explicitInfo.label)
                ? MemberNameToLabel(memberName, token)
                : explicitInfo.label;

            var memberPascalName = MakeFirstCharUpper(memberName);
            var generatedClassName = $"Binding{memberPascalName}";
            var callbackMethodName = memberPascalName;

            using var argNamesBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var arg in typeArgs)
            {
                token.ThrowIfCancellationRequested();

                argNamesBuilder.Add(arg.ToFullName());
            }

            var qualifiedName = string.IsNullOrEmpty(userNamespace)
                ? $"{userClassName}+{generatedClassName}"
                : $"{userNamespace}.{userClassName}+{generatedClassName}";

            var skip = compilation.GetTypeByMetadataName(qualifiedName) != null;

            builder.Add(new CommandBindingSpec {
                memberName = memberName,
                memberPascalName = memberPascalName,
                actionTypeArgs = argNamesBuilder.ToImmutable().AsEquatableArray(),
                wrapperTypeName = explicitInfo.wrapperTypeName ?? string.Empty,
                label = label,
                callbackMethodName = callbackMethodName,
                generatedClassName = generatedClassName,
                skipGeneration = skip,
                isObsolete = isObsolete && !isExplicit,
                obsoleteMessage = isObsolete && !isExplicit ? obsoleteMessage : string.Empty,
                isUnityEvent = isUnityEvent,
                delegateFullTypeName = isDelegate ? memberType.ToFullName() : string.Empty,
            });

            return true;
        }

        private static bool TryCollectPropertyFromMethod(
              IMethodSymbol method
            , CancellationToken token
            , string memberName
            , (bool useCustomSetter, string label) explicitInfo
            , string userClassName
            , string userNamespace
            , HashSet<string> seen
            , Compilation compilation
            , ImmutableArrayBuilder<PropertyBindingSpec> builder
        )
        {
            if (method.IsStatic || !method.ReturnsVoid || method.Parameters.Length != 1)
            {
                return false;
            }

            var paramType = method.Parameters[0].Type;
            var isUnityEvent = paramType.InheritsFromType(UNITY_EVENT_BASE_TYPE);
            var isDelegate = isUnityEvent == false && paramType.TypeKind == TypeKind.Delegate;

            if (isUnityEvent || isDelegate)
            {
                return false;
            }

            if (seen.Add(memberName) == false)
            {
                return false;
            }

            var label = string.IsNullOrEmpty(explicitInfo.label)
                ? MemberNameToLabel(memberName, token)
                : explicitInfo.label;

            var memberPascalName = MakeFirstCharUpper(memberName);
            var generatedClassName = $"Binding{memberPascalName}";
            var setterMethodName = memberPascalName;

            var propFullTypeName = paramType.ToFullName();
            var needsIn = NeedsInModifier(paramType, token);

            var variantConverterPropertyName = propFullTypeName == BinderSpec.VARIANT_TYPE
                ? string.Empty
                : paramType.ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();

            var qualifiedName = string.IsNullOrEmpty(userNamespace)
                ? $"{userClassName}+{generatedClassName}"
                : $"{userNamespace}.{userClassName}+{generatedClassName}";

            var skip = compilation.GetTypeByMetadataName(qualifiedName) != null;

            builder.Add(new PropertyBindingSpec {
                memberName = memberName,
                memberPascalName = memberPascalName,
                propFullTypeName = propFullTypeName,
                needsInModifier = needsIn,
                setterMethod = explicitInfo.useCustomSetter ? string.Empty : memberName,
                label = label,
                setterMethodName = setterMethodName,
                generatedClassName = generatedClassName,
                skipGeneration = skip,
                variantConverterPropertyName = variantConverterPropertyName,
                isObsolete = false,
                obsoleteMessage = string.Empty,
                useCustomSetter = explicitInfo.useCustomSetter,
                customSetterPartialMethodName = explicitInfo.useCustomSetter
                    ? DeriveCustomSetterPartialName(memberName)
                    : string.Empty,
            });

            return true;
        }

        private static IMethodSymbol FindOneParamVoidMethod(
              INamedTypeSymbol type
            , string memberName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var current = type as ITypeSymbol;

            while (current != null)
            {
                token.ThrowIfCancellationRequested();

                foreach (var member in current.GetMembers(memberName))
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IMethodSymbol method
                        && method.DeclaredAccessibility == Accessibility.Public
                        && !method.IsStatic
                        && method.ReturnsVoid
                        && method.Parameters.Length == 1)
                    {
                        return method;
                    }
                }

                current = current.BaseType;
            }

            return null;
        }

        private static string DeriveCustomSetterPartialName(string memberName)
        {
            if (memberName.Length > 3
                && memberName.StartsWith("Set", StringComparison.Ordinal)
                && char.IsUpper(memberName[3])
            )
            {
                return $"Set_{memberName.Substring(3)}";
            }

            return $"Set_{MakeFirstCharUpper(memberName)}";
        }

        private static (bool isObsolete, string message) GetObsoleteInfo(ISymbol symbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            const string OBSOLETE_ATTRIBUTE = "global::System.ObsoleteAttribute";

            foreach (var attr in symbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attr.AttributeClass.IsType(OBSOLETE_ATTRIBUTE, token))
                {
                    var message = attr.ConstructorArguments.Length >= 1
                        ? attr.ConstructorArguments[0].Value as string ?? string.Empty
                        : string.Empty;

                    return (true, message);
                }
            }

            return (false, string.Empty);
        }

        private static INamedTypeSymbol FindUnityEventBase(ITypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var walk = type as INamedTypeSymbol;

            while (walk != null)
            {
                token.ThrowIfCancellationRequested();

                if (walk is {
                    Name: "UnityEvent",
                    ContainingNamespace: {
                        Name: "Events",
                        ContainingNamespace.Name: "UnityEngine",
                    },
                })
                {
                    return walk;
                }

                walk = walk.BaseType;
            }

            return null;
        }

        private static ImmutableArray<ITypeSymbol> GetDelegateInvokeParameters(
              ITypeSymbol delegateType
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (delegateType is not INamedTypeSymbol named)
            {
                return ImmutableArray<ITypeSymbol>.Empty;
            }

            foreach (var member in named.GetMembers("Invoke"))
            {
                token.ThrowIfCancellationRequested();

                if (member is IMethodSymbol invoke)
                {
                    var count = invoke.Parameters.Length;

                    if (count == 0)
                    {
                        return ImmutableArray<ITypeSymbol>.Empty;
                    }

                    using var builder = ImmutableArrayBuilder<ITypeSymbol>.Rent();

                    foreach (var param in invoke.Parameters)
                    {
                        token.ThrowIfCancellationRequested();

                        builder.Add(param.Type);
                    }

                    return builder.ToImmutable();
                }
            }

            return ImmutableArray<ITypeSymbol>.Empty;
        }

        private static ISymbol FindMember(INamedTypeSymbol type, string memberName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var current = type as ITypeSymbol;

            while (current != null)
            {
                token.ThrowIfCancellationRequested();

                foreach (var member in current.GetMembers(memberName))
                {
                    token.ThrowIfCancellationRequested();

                    if (member.DeclaredAccessibility == Accessibility.Public)
                    {
                        return member;
                    }
                }

                current = current.BaseType;
            }

            return null;
        }

        internal static string MemberNameToLabel(string memberName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(memberName))
            {
                return memberName;
            }

            var sb = new StringBuilder(memberName.Length + 8);

            for (var i = 0; i < memberName.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var c = memberName[i];

                if (i == 0)
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
                else
                {
                    if (char.IsUpper(c) && char.IsLower(memberName[i - 1]))
                    {
                        sb.Append(' ');
                    }

                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static bool NeedsInModifier(ITypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (type == null || type.IsReferenceType || type.TypeKind == TypeKind.Pointer)
            {
                return false;
            }

            return EstimateTypeSize(type, token) >= 8;
        }

        private static int EstimateTypeSize(ITypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (type == null)
            {
                return 0;
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                var underlyingSpecial = type is INamedTypeSymbol e
                    ? e.EnumUnderlyingType?.SpecialType ?? SpecialType.System_Int32
                    : SpecialType.System_Int32;

                return GetPrimitiveSize(underlyingSpecial);
            }

            var primitiveSize = GetPrimitiveSize(type.SpecialType);

            if (primitiveSize > 0)
            {
                return primitiveSize;
            }

            if (type.TypeKind == TypeKind.Struct && type is INamedTypeSymbol named)
            {
                var total = 0;

                foreach (var member in named.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst)
                    {
                        // Reference-type fields are always an 8-byte pointer on 64-bit
                        total += field.Type.IsReferenceType ? 8 : EstimateTypeSize(field.Type, token);
                    }
                }

                return total;
            }

            return 0;
        }

        private static int GetPrimitiveSize(SpecialType special)
        {
            return special switch {
                SpecialType.System_Boolean => 1,
                SpecialType.System_Byte => 1,
                SpecialType.System_SByte => 1,
                SpecialType.System_Char => 2,
                SpecialType.System_Int16 => 2,
                SpecialType.System_UInt16 => 2,
                SpecialType.System_Int32 => 4,
                SpecialType.System_UInt32 => 4,
                SpecialType.System_Single => 4,
                SpecialType.System_Int64 => 8,
                SpecialType.System_UInt64 => 8,
                SpecialType.System_Double => 8,
                SpecialType.System_Decimal => 16,
                _ => 0,
            };
        }

        private static string MakeFirstCharUpper(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (char.IsUpper(value[0]))
            {
                return value;
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SCM = global::System.ComponentModel;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using ETA = global::EncosyTower.Annotations;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETMCM = global::EncosyTower.Mvvm.ComponentModel;");
            p.PrintLine("using ETMI = global::EncosyTower.Mvvm.Input;");
            p.PrintLine("using ETMVB = global::EncosyTower.Mvvm.ViewBinding;");
            p.PrintLine("using ETMVBC = global::EncosyTower.Mvvm.ViewBinding.Components;");
            p.PrintLine("using ETMVBSG = global::EncosyTower.Mvvm.ViewBinding.SourceGen;");
            p.PrintLine("using ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintLine("using UEE = global::UnityEngine.Events;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public readonly bool Equals(MonoBinderSpec other)
            => string.Equals(userClassName, other.userClassName, StringComparison.Ordinal)
            && string.Equals(userNamespace, other.userNamespace, StringComparison.Ordinal)
            && string.Equals(componentFullTypeName, other.componentFullTypeName, StringComparison.Ordinal)
            && string.Equals(componentLabelName, other.componentLabelName, StringComparison.Ordinal)
            && string.Equals(preprocessorGuard, other.preprocessorGuard, StringComparison.Ordinal)
            && propertyBindings.Equals(other.propertyBindings)
            && commandBindings.Equals(other.commandBindings)
            && isOuterClassSealed == other.isOuterClassSealed
            && string.Equals(outerTypeIdentifier, other.outerTypeIdentifier, StringComparison.Ordinal)
            && userBindingPropertyRefs.Equals(other.userBindingPropertyRefs)
            && userBindingCommandRefs.Equals(other.userBindingCommandRefs)
            && outerNonVariantTypes.Equals(other.outerNonVariantTypes)
            && hasOnBindPropertyFailedMethod == other.hasOnBindPropertyFailedMethod
            && hasOnBindCommandFailedMethod == other.hasOnBindCommandFailedMethod
            ;

        public readonly override bool Equals(object obj)
            => obj is MonoBinderSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  userClassName
                , userNamespace
                , componentFullTypeName
                , componentLabelName
                , preprocessorGuard
                , isOuterClassSealed
                , outerTypeIdentifier
            )
            .Add(propertyBindings)
            .Add(commandBindings)
            .Add(userBindingPropertyRefs)
            .Add(userBindingCommandRefs)
            .Add(outerNonVariantTypes)
            .Add(hasOnBindPropertyFailedMethod)
            .Add(hasOnBindCommandFailedMethod)
            ;

        private struct OuterBinderPropertyScanEntry
        {
            public IMethodSymbol method;
            public string paramFullTypeName;
            public string variantConverterPropName;
            public RefKind paramRefKind;
            public bool isParameterTypeVariant;
        }

        private struct OuterBinderCommandScanEntry
        {
            public IMethodSymbol method;
            public string paramFullTypeName;
            public string paramName;
            public RefKind paramRefKind;
        }
    }
}
