using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Common.Mvvm.Common;
using EncosyTower.SourceGen.Generators.Mvvm.Binders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    public partial struct MonoBinderDeclaration : IEquatable<MonoBinderDeclaration>
    {
        private const string MONO_BINDER_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBinderAttribute";
        private const string MONO_BINDING_PROP_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBindingPropertyAttribute";
        private const string MONO_BINDING_CMD_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBindingCommandAttribute";
        private const string MONO_BINDING_EXCLUDE_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBindingExcludeAttribute";
        private const string MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.Components.MonoBinderExcludeParentAttribute";
        private const string UNITY_EVENT_BASE_TYPE = "global::UnityEngine.Events.UnityEventBase";
        private const string UNITY_OBJECT_TYPE = "global::UnityEngine.Object";

        /// <summary>Excluded from <see cref="Equals"/> and <see cref="GetHashCode"/> —
        /// location is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string openingSource;
        public string closingSource;
        public string hintName;
        public string sourceFilePath;

        public string userClassName;
        public string userNamespace;
        public string componentFullTypeName;
        public string componentLabelName;
        public string preprocessorGuard;

        public EquatableArray<PropertyBindingInfo> propertyBindings;
        public EquatableArray<CommandBindingInfo> commandBindings;

        // IBinder: outer user class
        public bool isOuterClassSealed;
        public string outerTypeIdentifier;
        public bool hasOnBindPropertyFailedMethod;
        public bool hasOnBindCommandFailedMethod;
        public EquatableArray<BinderDeclaration.BindingPropertyInfo> userBindingPropertyRefs;
        public EquatableArray<BinderDeclaration.BindingCommandInfo> userBindingCommandRefs;
        public EquatableArray<BinderDeclaration.NonVariantTypeInfo> outerNonVariantTypes;

        public readonly bool IsValid
            => string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(userClassName) == false
            && string.IsNullOrEmpty(componentFullTypeName) == false;

        public static MonoBinderDeclaration Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax
                || context.TargetSymbol is not INamedTypeSymbol userClassSymbol
            )
            {
                return default;
            }

            // --- Extract MonoBinderAttribute data ---
            var monoBinderAttr = userClassSymbol.GetAttribute(MONO_BINDER_ATTRIBUTE);

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

            // --- Compute opening/closing namespace source ---
            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var userClassName = classSyntax.Identifier.Text;
            var userNamespace = userClassSymbol.ContainingNamespace is { IsGlobalNamespace: false } ns
                ? ns.ToDisplayString()
                : string.Empty;

            var componentFullTypeName = componentType.ToFullName();
            var componentLabelName = MemberNameToLabel(componentType.Name);

            var compilation = context.SemanticModel.Compilation;

            // --- Collect [ExcludeMonoBinding] member names ---
            var excludedMembers = new HashSet<string>(StringComparer.Ordinal);

            foreach (var attrData in userClassSymbol.GetAttributes(MONO_BINDING_EXCLUDE_ATTRIBUTE))
            {
                if (attrData.ConstructorArguments.Length < 1
                    || attrData.ConstructorArguments[0].Value is not string excludedName
                    || string.IsNullOrEmpty(excludedName)
                )
                {
                    continue;
                }

                excludedMembers.Add(excludedName);
            }

            // --- Collect [MonoBinderExcludeParent] parent type to stop the walk at ---
            INamedTypeSymbol excludedParentType = null;
            {
                var excludeParentAttr = userClassSymbol.GetAttribute(MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE);

                if (excludeParentAttr != null
                    && excludeParentAttr.ConstructorArguments.Length >= 1
                    && excludeParentAttr.ConstructorArguments[0].Value is INamedTypeSymbol excludedParent
                )
                {
                    excludedParentType = excludedParent;
                }
            }

            // --- Collect explicit [MonoBindingProperty] / [MonoBindingCommand] overrides ---
            var explicitProps = new Dictionary<string, (bool useCustomSetter, string label)>(StringComparer.Ordinal);
            var explicitCmds = new Dictionary<string, (string wrapperTypeName, string label)>(StringComparer.Ordinal);

            foreach (var attrData in userClassSymbol.GetAttributes(MONO_BINDING_PROP_ATTRIBUTE))
            {
                if (attrData.ConstructorArguments.Length < 1
                    || attrData.ConstructorArguments[0].Value is not string memberName
                    || string.IsNullOrEmpty(memberName)
                )
                {
                    continue;
                }

                var useCustomSetter = false;
                var labelOverride = string.Empty;

                foreach (var namedArg in attrData.NamedArguments)
                {
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

            foreach (var attrData in userClassSymbol.GetAttributes(MONO_BINDING_CMD_ATTRIBUTE))
            {
                if (attrData.ConstructorArguments.Length < 1
                    || attrData.ConstructorArguments[0].Value is not string memberName
                    || string.IsNullOrEmpty(memberName)
                )
                {
                    continue;
                }

                var wrapperTypeName = string.Empty;
                var labelOverride = string.Empty;

                foreach (var namedArg in attrData.NamedArguments)
                {
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

            // --- Walk component inheritance chain and collect members ---
            using var propBuilder = ImmutableArrayBuilder<PropertyBindingInfo>.Rent();
            using var cmdBuilder  = ImmutableArrayBuilder<CommandBindingInfo>.Rent();

            // Track members already seen to avoid duplicates from inherited re-declarations
            var seenPropMembers = new HashSet<string>(StringComparer.Ordinal);
            var seenCmdMembers  = new HashSet<string>(StringComparer.Ordinal);

            var current = componentType as ITypeSymbol;

            while (current != null && current.HasFullName(UNITY_OBJECT_TYPE) == false)
            {
                if (excludedParentType != null
                    && SymbolEqualityComparer.Default.Equals(current, excludedParentType)
                )
                {
                    break;
                }

                token.ThrowIfCancellationRequested();

                var members = current.GetMembers();

                foreach (var member in members)
                {
                    if (member.DeclaredAccessibility != Accessibility.Public || member.IsStatic)
                    {
                        continue;
                    }

                    if (member is IPropertySymbol prop)
                    {
                        var tryProperty = TryCollectProperty(
                              prop
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
                                  prop.Name
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
                              field.Name
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
                              evt.Name
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

            // --- Also add any explicit commands that weren't auto-discovered ---
            foreach (var kvp in explicitCmds)
            {
                if (seenCmdMembers.Contains(kvp.Key))
                {
                    continue;
                }

                if (excludedMembers.Contains(kvp.Key))
                {
                    continue;
                }

                // Find the member type on the component to build the action type args
                var memberSymbol = FindMember(componentType, kvp.Key);

                if (memberSymbol == null)
                {
                    continue;
                }

                ITypeSymbol memberType = memberSymbol switch {
                    IFieldSymbol fs    => fs.Type,
                    IPropertySymbol ps => ps.Type,
                    IEventSymbol es    => es.Type,
                    _                  => null,
                };

                if (memberType == null)
                {
                    continue;
                }

                TryCollectCommand(
                      kvp.Key
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

            // --- Also add any explicit properties that weren't auto-discovered (e.g. from excluded parent types) ---
            foreach (var kvp in explicitProps)
            {
                if (seenPropMembers.Contains(kvp.Key))
                {
                    continue;
                }

                var memberSymbol = FindMember(componentType, kvp.Key);

                if (memberSymbol is IPropertySymbol propSymbol)
                {
                    TryCollectProperty(
                          propSymbol
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
                    // The key may be a 1-parameter void method (e.g. SetActive) rather than a property.
                    var methodSymbol = FindOneParamVoidMethod(componentType, kvp.Key);

                    if (methodSymbol != null)
                    {
                        TryCollectPropertyFromMethod(
                              methodSymbol
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

            var monoBinderBaseSymbol      = compilation.GetTypeByMetadataName("EncosyTower.Mvvm.ViewBinding.Components.MonoBinder`1");
            var monoBindingPropBaseSymbol = compilation.GetTypeByMetadataName("EncosyTower.Mvvm.ViewBinding.Components.MonoBindingProperty`1");
            var monoBindingCmdBaseSymbol  = compilation.GetTypeByMetadataName("EncosyTower.Mvvm.ViewBinding.Components.MonoBindingCommand`1");
            var isOuterClassSealed  = userClassSymbol.IsSealed;
            var outerTypeIdentifier = userClassSymbol.ToValidIdentifier();

            // --- IBinder extraction: detect inherited OnBindPropertyFailed / OnBindCommandFailed ---
            var hasOnBindPropertyFailedMethod = false;
            var hasOnBindCommandFailedMethod  = false;
            {
                var walkType      = userClassSymbol;
                var isCurrentType = true;

                while (walkType != null)
                {
                    foreach (var member in walkType.GetMembers("OnBindPropertyFailed"))
                    {
                        if (member is IMethodSymbol checkMethod
                            && isCurrentType == false
                            && checkMethod.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
                            && checkMethod.Parameters.Length == 1
                            && checkMethod.Parameters[0].Type.HasFullName(BinderDeclaration.BINDING_PROPERTY)
                        )
                        {
                            hasOnBindPropertyFailedMethod = true;
                            break;
                        }
                    }

                    foreach (var member in walkType.GetMembers("OnBindCommandFailed"))
                    {
                        if (member is IMethodSymbol checkMethod
                            && isCurrentType == false
                            && checkMethod.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
                            && checkMethod.Parameters.Length == 1
                            && checkMethod.Parameters[0].Type.HasFullName(BinderDeclaration.BINDING_COMMAND)
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

            // --- IBinder extraction: scan user class for [BindingProperty] / [BindingCommand] methods ---
            var outerBindingPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            var outerConverterNames       = new HashSet<string>(StringComparer.Ordinal);
            var outerBindingCommandNames  = new HashSet<string>(StringComparer.Ordinal);
            var outerNonVariantTypeFilter = new Dictionary<string, BinderDeclaration.NonVariantTypeInfo>(StringComparer.Ordinal);

            var semanticModel = context.SemanticModel;

            using var tempOuterPropMethods = ImmutableArrayBuilder<OuterBinderPropertyScanEntry>.Rent();
            using var tempOuterCmdMethods  = ImmutableArrayBuilder<OuterBinderCommandScanEntry>.Rent();

            foreach (var outerMember in userClassSymbol.GetMembers())
            {
                if (outerMember is IMethodSymbol outerMethod)
                {
                    var bindingPropAttr = outerMethod.GetAttribute(BinderDeclaration.BINDING_PROPERTY_ATTRIBUTE);
                    var bindingCmdAttr  = outerMethod.GetAttribute(BinderDeclaration.BINDING_COMMAND_ATTRIBUTE);

                    if (bindingPropAttr != null && outerMethod.Parameters.Length < 2)
                    {
                        var outerParamType         = string.Empty;
                        var outerVarConvPropName   = string.Empty;
                        var outerParamRefKind      = RefKind.None;
                        var outerIsParamVariant    = true;

                        if (outerMethod.Parameters.Length == 1)
                        {
                            var outerParam = outerMethod.Parameters[0];

                            if (outerParam.RefKind is not (RefKind.None or RefKind.In or RefKind.Ref))
                            {
                                continue;
                            }

                            outerParamRefKind   = outerParam.RefKind;
                            outerParamType      = outerParam.Type.ToFullName();
                            outerIsParamVariant = outerParamType == BinderDeclaration.VARIANT_TYPE;

                            if (!outerIsParamVariant)
                            {
                                outerVarConvPropName = outerParam.Type
                                    .ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();
                            }
                        }

                        tempOuterPropMethods.Add(new OuterBinderPropertyScanEntry {
                            method                  = outerMethod,
                            paramFullTypeName        = outerParamType,
                            paramRefKind             = outerParamRefKind,
                            variantConverterPropName = outerVarConvPropName,
                            isParameterTypeVariant   = outerIsParamVariant,
                        });

                        if (!outerIsParamVariant
                            && !string.IsNullOrEmpty(outerParamType)
                            && !outerNonVariantTypeFilter.ContainsKey(outerParamType)
                        )
                        {
                            outerNonVariantTypeFilter[outerParamType] = new BinderDeclaration.NonVariantTypeInfo {
                                fullTypeName          = outerParamType,
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
                        var outerCmdParamType     = string.Empty;
                        var outerCmdParamRefKind  = RefKind.None;
                        var outerCmdParamName     = string.Empty;

                        if (outerMethod.Parameters.Length == 1
                            && outerMethod.Parameters[0].RefKind is RefKind.None or RefKind.In or RefKind.Ref
                        )
                        {
                            var outerParam        = outerMethod.Parameters[0];
                            outerCmdParamType     = outerParam.Type.ToFullName();
                            outerCmdParamRefKind  = outerParam.RefKind;
                            outerCmdParamName     = outerParam.Name;
                        }

                        tempOuterCmdMethods.Add(new OuterBinderCommandScanEntry {
                            method           = outerMethod,
                            paramFullTypeName = outerCmdParamType,
                            paramRefKind     = outerCmdParamRefKind,
                            paramName        = outerCmdParamName,
                        });
                    }

                    continue;
                }

                if (outerMember is IFieldSymbol outerField)
                {
                    var outerFieldTypeName = outerField.Type.ToFullName();

                    if (outerFieldTypeName == BinderDeclaration.BINDING_PROPERTY)
                    {
                        outerBindingPropertyNames.Add(outerField.Name);
                    }
                    else if (outerFieldTypeName == BinderDeclaration.CONVERTER)
                    {
                        outerConverterNames.Add(outerField.Name);
                    }
                    else if (outerFieldTypeName == BinderDeclaration.BINDING_COMMAND)
                    {
                        outerBindingCommandNames.Add(outerField.Name);
                    }
                }
            }

            // Second pass: gather forwarded attributes for outer class binding methods
            using var outerPropRefBuilder      = ImmutableArrayBuilder<BinderDeclaration.BindingPropertyInfo>.Rent();
            using var outerCmdRefBuilder       = ImmutableArrayBuilder<BinderDeclaration.BindingCommandInfo>.Rent();
            using var outerDiagnostics         = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            foreach (var entry in tempOuterPropMethods.WrittenSpan)
            {
                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , outerDiagnostics
                    , out var propForwardedFieldAttrs
                    , out _
                    , DiagnosticDescriptors.InvalidFieldTargetedAttributeOnBindingPropertyMethod
                );

                outerPropRefBuilder.Add(new BinderDeclaration.BindingPropertyInfo {
                    methodName                   = entry.method.Name,
                    paramFullTypeName             = entry.paramFullTypeName,
                    paramRefKind                 = entry.paramRefKind,
                    variantConverterPropertyName  = entry.variantConverterPropName,
                    isParameterTypeVariant        = entry.isParameterTypeVariant,
                    skipBindingProperty           = outerBindingPropertyNames.Contains($"_bindingFieldFor{entry.method.Name}"),
                    skipConverter                 = outerConverterNames.Contains($"_converterFor{entry.method.Name}"),
                    forwardedFieldAttributes      = propForwardedFieldAttrs.AsEquatableArray(),
                });
            }

            foreach (var entry in tempOuterCmdMethods.WrittenSpan)
            {
                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , false
                    , outerDiagnostics
                    , out var cmdForwardedFieldAttrs
                    , out _
                    , DiagnosticDescriptors.InvalidFieldTargetedAttributeOnBindingCommandMethod
                );

                outerCmdRefBuilder.Add(new BinderDeclaration.BindingCommandInfo {
                    methodName               = entry.method.Name,
                    paramFullTypeName        = entry.paramFullTypeName,
                    paramRefKind             = entry.paramRefKind,
                    paramName                = entry.paramName,
                    skipBindingCommand       = outerBindingCommandNames.Contains($"_bindingCommandFor{entry.method.Name}"),
                    forwardedFieldAttributes = cmdForwardedFieldAttrs.AsEquatableArray(),
                });
            }

            using var outerNonVariantTypesBuilder = ImmutableArrayBuilder<BinderDeclaration.NonVariantTypeInfo>.Rent();

            foreach (var nvt in outerNonVariantTypeFilter.Values)
                outerNonVariantTypesBuilder.Add(nvt);

            // --- Hint name and source file path ---
            var syntaxTree  = classSyntax.SyntaxTree;
            var fileTypeName = userClassSymbol.ToFileName();

            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  MonoBinderGenerator.GENERATOR_NAME
                , classSyntax
                , fileTypeName
            );

            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  compilation.Assembly.Name
                , MonoBinderGenerator.GENERATOR_NAME
                , fileTypeName
            );

            return new MonoBinderDeclaration {
                location              = LocationInfo.From(classSyntax.GetLocation()),
                openingSource         = openingSource,
                closingSource         = closingSource,
                hintName              = hintName,
                sourceFilePath        = sourceFilePath,
                userClassName         = userClassName,
                userNamespace         = userNamespace,
                componentFullTypeName  = componentFullTypeName,
                componentLabelName    = componentLabelName,
                preprocessorGuard     = preprocessorGuard,
                propertyBindings      = propBuilder.ToImmutable().AsEquatableArray(),
                commandBindings       = cmdBuilder.ToImmutable().AsEquatableArray(),
                isOuterClassSealed        = isOuterClassSealed,
                outerTypeIdentifier       = outerTypeIdentifier,
                hasOnBindPropertyFailedMethod = hasOnBindPropertyFailedMethod,
                hasOnBindCommandFailedMethod  = hasOnBindCommandFailedMethod,
                userBindingPropertyRefs   = outerPropRefBuilder.ToImmutable().AsEquatableArray(),
                userBindingCommandRefs    = outerCmdRefBuilder.ToImmutable().AsEquatableArray(),
                outerNonVariantTypes      = outerNonVariantTypesBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        // -----------------------------------------------------------------------
        //  Private helpers
        // -----------------------------------------------------------------------

        private static bool TryCollectProperty(
              IPropertySymbol prop
            , string userClassName
            , string userNamespace
            , Dictionary<string, (bool useCustomSetter, string label)> explicitProps
            , HashSet<string> seen
            , HashSet<string> excludedMembers
            , bool excludeObsolete
            , Compilation compilation
            , ImmutableArrayBuilder<PropertyBindingInfo> builder
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
            var isDelegate   = isUnityEvent == false && prop.Type.TypeKind == TypeKind.Delegate;

            // Skip properties whose type is or derives from UnityEventBase or is a C# delegate
            // (those should go through MonoBindingCommand, not MonoBindingProperty)
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
            var (isObsolete, obsoleteMessage) = GetObsoleteInfo(prop);

            if (isObsolete && excludeObsolete && !isExplicit)
            {
                return false;
            }

            explicitProps.TryGetValue(memberName, out var explicitInfo);

            var label = string.IsNullOrEmpty(explicitInfo.label)
                ? MemberNameToLabel(memberName)
                : explicitInfo.label;

            var memberPascalName = MakeFirstCharUpper(memberName);
            var generatedClassName = $"Binding{memberPascalName}";
            var setterMethodName = $"Set{memberPascalName}";
            var propFullTypeName = prop.Type.ToFullName();
            var needsIn = NeedsInModifier(prop.Type);

            var variantConverterPropertyName = propFullTypeName == BinderDeclaration.VARIANT_TYPE
                ? string.Empty
                : prop.Type.ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();

            var qualifiedName = string.IsNullOrEmpty(userNamespace)
                ? $"{userClassName}+{generatedClassName}"
                : $"{userNamespace}.{userClassName}+{generatedClassName}";

            var skip = compilation.GetTypeByMetadataName(qualifiedName) != null;

            builder.Add(new PropertyBindingInfo {
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
              string memberName
            , ITypeSymbol memberType
            , string userClassName
            , string userNamespace
            , Dictionary<string, (string wrapperTypeName, string label)> explicitCmds
            , HashSet<string> seen
            , HashSet<string> excludedMembers
            , bool excludeObsolete
            , Compilation compilation
            , ImmutableArrayBuilder<CommandBindingInfo> builder
            , ISymbol memberSymbol = null
            , bool forceExplicit = false
        )
        {
            if (excludedMembers.Contains(memberName))
            {
                return false;
            }

            var isUnityEvent = memberType.InheritsFromType(UNITY_EVENT_BASE_TYPE);
            var isDelegate   = isUnityEvent == false && memberType.TypeKind == TypeKind.Delegate;

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
                ? GetObsoleteInfo(memberSymbol)
                : (false, string.Empty);

            if (isObsolete && excludeObsolete && !isExplicit)
            {
                return false;
            }

            // Walk up to find the direct UnityEvent<...> base to get type args;
            // for C# delegates/events, extract parameter types from the Invoke method.
            ImmutableArray<ITypeSymbol> typeArgs;

            if (isUnityEvent)
            {
                var unityEventBase = FindUnityEventBase(memberType);
                typeArgs = unityEventBase?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
            }
            else
            {
                typeArgs = GetDelegateInvokeParameters(memberType);
            }

            // For auto-discovery, skip multi-param events without an explicit wrapper type
            explicitCmds.TryGetValue(memberName, out var explicitInfo);

            if (forceExplicit == false
                && typeArgs.Length > 1
                && string.IsNullOrEmpty(explicitInfo.wrapperTypeName)
            )
            {
                return false;
            }

            var label = string.IsNullOrEmpty(explicitInfo.label)
                ? MemberNameToLabel(memberName)
                : explicitInfo.label;

            var memberPascalName = MakeFirstCharUpper(memberName);
            var generatedClassName = $"Binding{memberPascalName}";
            var callbackMethodName = memberPascalName;

            using var argNamesBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var arg in typeArgs)
            {
                argNamesBuilder.Add(arg.ToFullName());
            }

            var qualifiedName = string.IsNullOrEmpty(userNamespace)
                ? $"{userClassName}+{generatedClassName}"
                : $"{userNamespace}.{userClassName}+{generatedClassName}";

            var skip = compilation.GetTypeByMetadataName(qualifiedName) != null;

            builder.Add(new CommandBindingInfo {
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

        /// <summary>
        /// Handles an explicit <c>[MonoBindingProperty("MethodName")]</c> where the key resolves to
        /// a public void method with exactly one parameter instead of a settable property.
        /// </summary>
        private static bool TryCollectPropertyFromMethod(
              IMethodSymbol method
            , string memberName
            , (bool useCustomSetter, string label) explicitInfo
            , string userClassName
            , string userNamespace
            , HashSet<string> seen
            , Compilation compilation
            , ImmutableArrayBuilder<PropertyBindingInfo> builder
        )
        {
            if (method.IsStatic || !method.ReturnsVoid || method.Parameters.Length != 1)
            {
                return false;
            }

            var paramType    = method.Parameters[0].Type;
            var isUnityEvent = paramType.InheritsFromType(UNITY_EVENT_BASE_TYPE);
            var isDelegate   = isUnityEvent == false && paramType.TypeKind == TypeKind.Delegate;

            // Delegate/event parameters belong in TryCollectCommand, not here.
            if (isUnityEvent || isDelegate)
            {
                return false;
            }

            if (seen.Add(memberName) == false)
            {
                return false;
            }

            var label = string.IsNullOrEmpty(explicitInfo.label)
                ? MemberNameToLabel(memberName)
                : explicitInfo.label;

            var memberPascalName   = MakeFirstCharUpper(memberName);
            var generatedClassName = $"Binding{memberPascalName}";

            // Use memberPascalName directly to avoid "SetSetActive" when the method already starts with "Set".
            var setterMethodName = memberPascalName;

            var propFullTypeName = paramType.ToFullName();
            var needsIn          = NeedsInModifier(paramType);

            var variantConverterPropertyName = propFullTypeName == BinderDeclaration.VARIANT_TYPE
                ? string.Empty
                : paramType.ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();

            var qualifiedName = string.IsNullOrEmpty(userNamespace)
                ? $"{userClassName}+{generatedClassName}"
                : $"{userNamespace}.{userClassName}+{generatedClassName}";

            var skip = compilation.GetTypeByMetadataName(qualifiedName) != null;

            builder.Add(new PropertyBindingInfo {
                memberName                    = memberName,
                memberPascalName              = memberPascalName,
                propFullTypeName              = propFullTypeName,
                needsInModifier               = needsIn,
                // When useCustomSetter==false the binding body calls targets[i].MethodName(value).
                // When useCustomSetter==true  the binding body calls OuterClass.Set_X(targets[i], value).
                setterMethod                  = explicitInfo.useCustomSetter ? string.Empty : memberName,
                label                         = label,
                setterMethodName              = setterMethodName,
                generatedClassName            = generatedClassName,
                skipGeneration                = skip,
                variantConverterPropertyName  = variantConverterPropertyName,
                isObsolete                    = false,
                obsoleteMessage               = string.Empty,
                useCustomSetter               = explicitInfo.useCustomSetter,
                customSetterPartialMethodName = explicitInfo.useCustomSetter
                    ? DeriveCustomSetterPartialName(memberName)
                    : string.Empty,
            });

            return true;
        }

        /// <summary>
        /// Finds the first public instance void method named <paramref name="memberName"/> with
        /// exactly one parameter declared on <paramref name="type"/> or any of its base types.
        /// </summary>
        private static IMethodSymbol FindOneParamVoidMethod(INamedTypeSymbol type, string memberName)
        {
            var current = type as ITypeSymbol;

            while (current != null)
            {
                foreach (var member in current.GetMembers(memberName))
                {
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

        /// <summary>
        /// Derives the name of the generated <c>private static partial void</c> setter method
        /// from a member name.
        /// <list type="bullet">
        ///   <item><c>"SetActive"</c>  → <c>"Set_Active"</c></item>
        ///   <item><c>"Activate"</c>   → <c>"Set_Activate"</c></item>
        ///   <item><c>"active"</c>     → <c>"Set_Active"</c></item>
        /// </list>
        /// </summary>
        private static string DeriveCustomSetterPartialName(string memberName)
        {
            // Strip a leading "Set" prefix when followed directly by an uppercase letter.
            if (memberName.Length > 3
                && memberName.StartsWith("Set", StringComparison.Ordinal)
                && char.IsUpper(memberName[3])
            )
            {
                return "Set_" + memberName.Substring(3);
            }

            return "Set_" + MakeFirstCharUpper(memberName);
        }

        private static (bool isObsolete, string message) GetObsoleteInfo(ISymbol symbol)
        {
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute")
                {
                    var message = attr.ConstructorArguments.Length >= 1
                        ? attr.ConstructorArguments[0].Value as string ?? string.Empty
                        : string.Empty;

                    return (true, message);
                }
            }

            return (false, string.Empty);
        }

        private static INamedTypeSymbol FindUnityEventBase(ITypeSymbol type)
        {
            var walk = type as INamedTypeSymbol;

            while (walk != null)
            {
                if (walk.Name == "UnityEvent"
                    && walk.ContainingNamespace?.ToDisplayString() == "UnityEngine.Events"
                )
                {
                    return walk;
                }

                walk = walk.BaseType;
            }

            return null;
        }

        private static ImmutableArray<ITypeSymbol> GetDelegateInvokeParameters(ITypeSymbol delegateType)
        {
            if (delegateType is not INamedTypeSymbol named)
            {
                return ImmutableArray<ITypeSymbol>.Empty;
            }

            foreach (var member in named.GetMembers("Invoke"))
            {
                if (member is IMethodSymbol invoke)
                {
                    var count = invoke.Parameters.Length;

                    if (count == 0)
                    {
                        return ImmutableArray<ITypeSymbol>.Empty;
                    }

                    var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(count);

                    foreach (var param in invoke.Parameters)
                    {
                        builder.Add(param.Type);
                    }

                    return builder.MoveToImmutable();
                }
            }

            return ImmutableArray<ITypeSymbol>.Empty;
        }

        private static ISymbol FindMember(INamedTypeSymbol type, string memberName)
        {
            var current = type as ITypeSymbol;

            while (current != null)
            {
                foreach (var member in current.GetMembers(memberName))
                {
                    if (member.DeclaredAccessibility == Accessibility.Public)
                    {
                        return member;
                    }
                }

                current = current.BaseType;
            }

            return null;
        }

        internal static string MemberNameToLabel(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                return memberName;
            }

            var sb = new StringBuilder(memberName.Length + 8);

            for (var i = 0; i < memberName.Length; i++)
            {
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

        /// <summary>
        /// Returns <see langword="true"/> when the value-type parameter is large enough
        /// (>= 8 bytes) that passing it via <c>in</c> avoids a meaningful copy.
        /// Reference types are already 8-byte pointers and never need <c>in</c>.
        /// </summary>
        private static bool NeedsInModifier(ITypeSymbol type)
        {
            if (type == null || type.IsReferenceType || type.TypeKind == TypeKind.Pointer)
            {
                return false;
            }

            return EstimateTypeSize(type) >= 8;
        }

        /// <summary>
        /// Estimates the in-memory byte size of a type.
        /// Reference-type fields inside a struct are counted as 8 bytes (64-bit pointer).
        /// Returns 0 for unknown / unresolvable types.
        /// </summary>
        private static int EstimateTypeSize(ITypeSymbol type)
        {
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
                    if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst)
                    {
                        // Reference-type fields are always an 8-byte pointer on 64-bit
                        total += field.Type.IsReferenceType ? 8 : EstimateTypeSize(field.Type);
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
                SpecialType.System_Byte    => 1,
                SpecialType.System_SByte   => 1,
                SpecialType.System_Char    => 2,
                SpecialType.System_Int16   => 2,
                SpecialType.System_UInt16  => 2,
                SpecialType.System_Int32   => 4,
                SpecialType.System_UInt32  => 4,
                SpecialType.System_Single  => 4,
                SpecialType.System_Int64   => 8,
                SpecialType.System_UInt64  => 8,
                SpecialType.System_Double  => 8,
                SpecialType.System_Decimal => 16,
                _                          => 0,
            };
        }

        private static string MakeFirstCharUpper(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (char.IsUpper(value[0]))
                return value;

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using EncosyTower.Annotations;");
            p.PrintLine("using EncosyTower.Common;");
            p.PrintLine("using EncosyTower.Mvvm.ComponentModel;");
            p.PrintLine("using EncosyTower.Mvvm.Input;");
            p.PrintLine("using EncosyTower.Mvvm.ViewBinding;");
            p.PrintLine("using EncosyTower.Mvvm.ViewBinding.Components;");
            p.PrintLine("using EncosyTower.Mvvm.ViewBinding.SourceGen;");
            p.PrintLine("using EncosyTower.Variants.Converters;");
            p.PrintLine("using UnityEngine;");
            p.PrintLine("using UnityEngine.Events;");
            p.PrintEndLine();
            p.PrintLine("using Converter = global::EncosyTower.Mvvm.ViewBinding.Converter;");
            p.PrintLine("using EditorBrowsableAttribute = global::System.ComponentModel.EditorBrowsableAttribute;");
            p.PrintLine("using EditorBrowsableState = global::System.ComponentModel.EditorBrowsableState;");
            p.PrintLine("using INotifyPropertyChanged = global::EncosyTower.Mvvm.ComponentModel.INotifyPropertyChanged;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        // -----------------------------------------------------------------------
        //  IEquatable
        // -----------------------------------------------------------------------

        public readonly bool Equals(MonoBinderDeclaration other)
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
            => obj is MonoBinderDeclaration other && Equals(other);

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

        // ── Private scan-entry helpers (used only during Extract for outer class IBinder) ──

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
