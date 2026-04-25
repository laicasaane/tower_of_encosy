using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Mvvm.MonoBinders
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class MonoBinderDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Mvvm.ViewBinding.Components";
        private const string MONO_BINDER_ATTRIBUTE = $"global::{NAMESPACE}.MonoBinderAttribute";
        private const string MONO_BINDING_PROP_ATTRIBUTE = $"global::{NAMESPACE}.MonoBindingPropertyAttribute";
        private const string MONO_BINDING_CMD_ATTRIBUTE = $"global::{NAMESPACE}.MonoBindingCommandAttribute";
        private const string MONO_BINDING_EXCLUDE_ATTRIBUTE = $"global::{NAMESPACE}.MonoBindingExcludeAttribute";
        private const string MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE = $"global::{NAMESPACE}.MonoBinderExcludeParentAttribute";
        private const string UNITY_OBJECT_TYPE = "global::UnityEngine.Object";
        private const string OBSOLETE_ATTRIBUTE = "global::System.ObsoleteAttribute";

        public static readonly DiagnosticDescriptor ComponentTypeMustInheritUnityObject = new(
              id: "SG_MONO_BINDER_0001"
            , title: "MonoBinder component type must inherit from UnityEngine.Object"
            , messageFormat: "\"{0}\" does not inherit from UnityEngine.Object. The type argument of [MonoBinder] must be a UnityEngine.Object subclass."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [MonoBinder(typeof(...))] must inherit from UnityEngine.Object."
        );

        public static readonly DiagnosticDescriptor BindingAttributeRequiresMonoBinder = new(
              id: "SG_MONO_BINDER_0002"
            , title: "[MonoBindingProperty], [MonoBindingCommand], [MonoBindingExclude], and [MonoBinderExcludeParent] require [MonoBinder]"
            , messageFormat: "\"{0}\" has [{1}] but is missing [MonoBinder]. Binding attributes are only valid on types that also carry [MonoBinder]."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[MonoBindingProperty], [MonoBindingCommand], [MonoBindingExclude], and [MonoBinderExcludeParent] are only meaningful on a class that also has [MonoBinder]."
        );

        public static readonly DiagnosticDescriptor ExcludedMemberNotFound = new(
              id: "SG_MONO_BINDER_0003"
            , title: "[MonoBindingExclude] member not found on component type"
            , messageFormat: "Member \"{0}\" was not found as a public non-static property, field, or event on component type \"{1}\"."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The member name passed to [MonoBindingExclude] does not match any public non-static property, field, or event on the component type."
        );

        public static readonly DiagnosticDescriptor ExcludeParentTypeIsComponentType = new(
              id: "SG_MONO_BINDER_0004"
            , title: "[MonoBinderExcludeParent] type must not be the component type itself"
            , messageFormat: "[MonoBinderExcludeParent]: \"{0}\" is the same as the component type and has no effect."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [MonoBinderExcludeParent] must not be the same as the component type."
        );

        public static readonly DiagnosticDescriptor ExcludeParentTypeNotInHierarchy = new(
              id: "SG_MONO_BINDER_0005"
            , title: "[MonoBinderExcludeParent] type must be a base type of the component type"
            , messageFormat: "[MonoBinderExcludeParent]: \"{0}\" is not a base type of component type \"{1}\"."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [MonoBinderExcludeParent] must be a base type (at any level) of the component type."
        );

        public static readonly DiagnosticDescriptor ObsoleteExplicitMemberWithExcludeObsolete = new(
              id: "SG_MONO_BINDER_0006"
            , title: "[MonoBinder(ExcludeObsolete = true)] conflict with explicit binding attribute on obsolete member"
            , messageFormat: "[MonoBinder(ExcludeObsolete = true)]: \"{0}\" is marked [Obsolete] on \"{1}\". Remove [{2}] or set ExcludeObsolete = false."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "When ExcludeObsolete = true, members marked [Obsolete] must not be explicitly included via [MonoBindingProperty] or [MonoBindingCommand]."
        );

        public static readonly DiagnosticDescriptor ExcludeObsoleteMemberRedundant = new(
              id: "SG_MONO_BINDER_0007"
            , title: "[MonoBindingExclude] is redundant when ExcludeObsolete = true covers the same member"
            , messageFormat: "[MonoBinder(ExcludeObsolete = true)]: [MonoBindingExclude(\"{0}\")] is unnecessary because ExcludeObsolete = true already excludes obsolete members."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Info
            , isEnabledByDefault: true
            , description: "When ExcludeObsolete = true, [MonoBindingExclude] on a member that is already obsolete is redundant."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  ComponentTypeMustInheritUnityObject
                , BindingAttributeRequiresMonoBinder
                , ExcludedMemberNotFound
                , ExcludeParentTypeIsComponentType
                , ExcludeParentTypeNotInHierarchy
                , ObsoleteExplicitMemberWithExcludeObsolete
                , ExcludeObsoleteMemberRedundant
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            AttributeData monoBinderAttr = null;
            var hasMonoBindingProp = false;
            var hasMonoBindingCmd = false;
            var hasMonoBindingExclude = false;
            var hasMonoBinderExcludeParent = false;
            AttributeData firstOrphanAttr = null;
            string firstOrphanAttrName = null;

            foreach (var attr in typeSymbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;

                if (attrClass is null)
                {
                    continue;
                }

                if (attrClass.IsType(MONO_BINDER_ATTRIBUTE))
                {
                    monoBinderAttr = attr;
                }
                else if (attrClass.IsType(MONO_BINDING_PROP_ATTRIBUTE))
                {
                    hasMonoBindingProp = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBindingProperty";
                    }
                }
                else if (attrClass.IsType(MONO_BINDING_CMD_ATTRIBUTE))
                {
                    hasMonoBindingCmd = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBindingCommand";
                    }
                }
                else if (attrClass.IsType(MONO_BINDING_EXCLUDE_ATTRIBUTE))
                {
                    hasMonoBindingExclude = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBindingExclude";
                    }
                }
                else if (attrClass.IsType(MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE))
                {
                    hasMonoBinderExcludeParent = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBinderExcludeParent";
                    }
                }
            }

            if (monoBinderAttr == null
                && (hasMonoBindingProp || hasMonoBindingCmd || hasMonoBindingExclude || hasMonoBinderExcludeParent)
                && firstOrphanAttr != null
            )
            {
                var location = firstOrphanAttr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      BindingAttributeRequiresMonoBinder
                    , location
                    , typeSymbol.Name
                    , firstOrphanAttrName
                ));

                return;
            }

            if (monoBinderAttr == null)
            {
                return;
            }

            if (monoBinderAttr.ConstructorArguments.Length < 1
                || monoBinderAttr.ConstructorArguments[0].Value is not INamedTypeSymbol componentType)
            {
                return;
            }

            var excludeObsolete = false;

            foreach (var namedArg in monoBinderAttr.NamedArguments)
            {
                if (string.Equals(namedArg.Key, "ExcludeObsolete", StringComparison.Ordinal)
                    && namedArg.Value.Value is bool boolValue
                )
                {
                    excludeObsolete = boolValue;
                    break;
                }
            }

            if (InheritsFromUnityObject(componentType) == false)
            {
                var location = monoBinderAttr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      ComponentTypeMustInheritUnityObject
                    , location
                    , componentType.ToDisplayString()
                ));
            }

            if (excludeObsolete)
            {
                foreach (var attr in typeSymbol.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;

                    if (attrClass is null)
                    {
                        continue;
                    }

                    if (attrClass.IsType(MONO_BINDING_PROP_ATTRIBUTE)
                        || attrClass.IsType(MONO_BINDING_CMD_ATTRIBUTE)
                    )
                    {
                        if (attr.ConstructorArguments.Length < 1
                            || attr.ConstructorArguments[0].Value is not string memberName
                            || string.IsNullOrEmpty(memberName)
                        )
                        {
                            continue;
                        }

                        if (IsObsoleteMember(componentType, memberName))
                        {
                            var attrName = attrClass.IsType(MONO_BINDING_PROP_ATTRIBUTE)
                                ? "MonoBindingProperty"
                                : "MonoBindingCommand";

                            var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                                ?? typeSymbol.Locations[0];

                            context.ReportDiagnostic(Diagnostic.Create(
                                  ObsoleteExplicitMemberWithExcludeObsolete
                                , location
                                , memberName
                                , componentType.ToDisplayString()
                                , attrName
                            ));
                        }
                    }
                    else if (attrClass.IsType(MONO_BINDING_EXCLUDE_ATTRIBUTE))
                    {
                        if (attr.ConstructorArguments.Length < 1
                            || attr.ConstructorArguments[0].Value is not string memberName
                            || string.IsNullOrEmpty(memberName)
                        )
                        {
                            continue;
                        }

                        if (IsObsoleteMember(componentType, memberName))
                        {
                            var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                                ?? typeSymbol.Locations[0];

                            context.ReportDiagnostic(Diagnostic.Create(
                                  ExcludeObsoleteMemberRedundant
                                , location
                                , memberName
                            ));
                        }
                    }
                }
            }

            if (hasMonoBinderExcludeParent)
            {
                foreach (var attr in typeSymbol.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;

                    if (attrClass is null || attrClass.IsType(MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE) == false)
                    {
                        continue;
                    }

                    if (attr.ConstructorArguments.Length < 1
                        || attr.ConstructorArguments[0].Value is not INamedTypeSymbol parentType
                    )
                    {
                        continue;
                    }

                    if (SymbolEqualityComparer.Default.Equals(parentType, componentType))
                    {
                        var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                            ?? typeSymbol.Locations[0];

                        context.ReportDiagnostic(Diagnostic.Create(
                              ExcludeParentTypeIsComponentType
                            , location
                            , parentType.ToDisplayString()
                        ));
                    }
                    else if (IsInBaseTypeChain(componentType, parentType) == false)
                    {
                        var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                            ?? typeSymbol.Locations[0];

                        context.ReportDiagnostic(Diagnostic.Create(
                              ExcludeParentTypeNotInHierarchy
                            , location
                            , parentType.ToDisplayString()
                            , componentType.ToDisplayString()
                        ));
                    }
                }
            }

            if (hasMonoBindingExclude == false)
            {
                return;
            }

            foreach (var attr in typeSymbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;

                if (attrClass is null || attrClass.IsType(MONO_BINDING_EXCLUDE_ATTRIBUTE) == false)
                {
                    continue;
                }

                if (attr.ConstructorArguments.Length < 1
                    || attr.ConstructorArguments[0].Value is not string excludedName
                    || string.IsNullOrEmpty(excludedName)
                )
                {
                    continue;
                }

                if (ComponentHasMember(componentType, excludedName) == false)
                {
                    var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                        ?? typeSymbol.Locations[0];

                    context.ReportDiagnostic(Diagnostic.Create(
                          ExcludedMemberNotFound
                        , location
                        , excludedName
                        , componentType.ToDisplayString()
                    ));
                }
            }
        }

        private static bool InheritsFromUnityObject(ITypeSymbol type)
        {
            var walk = type;

            while (walk != null)
            {
                if (walk.IsType(UNITY_OBJECT_TYPE))
                {
                    return true;
                }

                walk = walk.BaseType;
            }

            return false;
        }

        private static bool IsInBaseTypeChain(INamedTypeSymbol componentType, INamedTypeSymbol parentType)
        {
            var walk = componentType.BaseType;

            while (walk != null)
            {
                if (walk.IsType(UNITY_OBJECT_TYPE))
                {
                    break;
                }

                if (SymbolEqualityComparer.Default.Equals(walk, parentType))
                {
                    return true;
                }

                walk = walk.BaseType;
            }

            return false;
        }

        private static bool ComponentHasMember(INamedTypeSymbol componentType, string memberName)
        {
            var current = componentType as ITypeSymbol;

            while (current != null)
            {
                foreach (var member in current.GetMembers(memberName))
                {
                    if (member.DeclaredAccessibility == Accessibility.Public
                        && member.IsStatic == false
                        && (member is IPropertySymbol || member is IFieldSymbol || member is IEventSymbol)
                    )
                    {
                        return true;
                    }
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsObsoleteMember(INamedTypeSymbol componentType, string memberName)
        {
            var current = componentType as ITypeSymbol;

            while (current != null)
            {
                foreach (var member in current.GetMembers(memberName))
                {
                    if (member.DeclaredAccessibility == Accessibility.Public
                        && member.IsStatic == false
                        && (member is IPropertySymbol
                            || member is IFieldSymbol
                            || member is IEventSymbol
                            || (member is IMethodSymbol ms && ms.ReturnsVoid && ms.Parameters.Length == 1)
                        )
                    )
                    {
                        foreach (var attr in member.GetAttributes())
                        {
                            if (attr.AttributeClass is { } attrib && attrib.IsType(OBSOLETE_ATTRIBUTE))
                            {
                                return true;
                            }
                        }
                    }
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}
