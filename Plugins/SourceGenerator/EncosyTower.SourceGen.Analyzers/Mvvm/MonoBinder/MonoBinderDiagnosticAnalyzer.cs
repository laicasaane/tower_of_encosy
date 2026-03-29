using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Analyzers.Mvvm.MonoBinders
{
    /// <summary>
    /// Reports validation diagnostics for types annotated with <c>[MonoBinder]</c>,
    /// <c>[MonoBindingProperty]</c>, <c>[MonoBindingCommand]</c>, and <c>[MonoBindingExclude]</c>.
    /// Kept in a <see cref="DiagnosticAnalyzer"/> rather than the generator to avoid
    /// invalidating the incremental source-gen cache on every edit.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class MonoBinderDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string MONO_BINDER_ATTRIBUTE = "EncosyTower.Mvvm.ViewBinding.Components.MonoBinderAttribute";
        private const string MONO_BINDING_PROP_ATTRIBUTE = "EncosyTower.Mvvm.ViewBinding.Components.MonoBindingPropertyAttribute";
        private const string MONO_BINDING_CMD_ATTRIBUTE = "EncosyTower.Mvvm.ViewBinding.Components.MonoBindingCommandAttribute";
        private const string MONO_BINDING_EXCLUDE_ATTRIBUTE = "EncosyTower.Mvvm.ViewBinding.Components.MonoBindingExcludeAttribute";
        private const string MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE = "EncosyTower.Mvvm.ViewBinding.Components.MonoBinderExcludeParentAttribute";
        private const string UNITY_OBJECT_TYPE = "UnityEngine.Object";

        public static readonly DiagnosticDescriptor ComponentTypeMustInheritUnityObject = new(
              id: "MONO_BINDER_0001"
            , title: "MonoBinder component type must inherit from UnityEngine.Object"
            , messageFormat: "\"{0}\" does not inherit from UnityEngine.Object. The type argument of [MonoBinder] must be a UnityEngine.Object subclass."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [MonoBinder(typeof(...))] must inherit from UnityEngine.Object."
        );

        public static readonly DiagnosticDescriptor BindingAttributeRequiresMonoBinder = new(
              id: "MONO_BINDER_0002"
            , title: "[MonoBindingProperty], [MonoBindingCommand], [MonoBindingExclude], and [MonoBinderExcludeParent] require [MonoBinder]"
            , messageFormat: "\"{0}\" has [{1}] but is missing [MonoBinder]. Binding attributes are only valid on types that also carry [MonoBinder]."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[MonoBindingProperty], [MonoBindingCommand], [MonoBindingExclude], and [MonoBinderExcludeParent] are only meaningful on a class that also has [MonoBinder]."
        );

        public static readonly DiagnosticDescriptor ExcludedMemberNotFound = new(
              id: "MONO_BINDER_0003"
            , title: "[MonoBindingExclude] member not found on component type"
            , messageFormat: "Member \"{0}\" was not found as a public non-static property, field, or event on component type \"{1}\"."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The member name passed to [MonoBindingExclude] does not match any public non-static property, field, or event on the component type."
        );

        public static readonly DiagnosticDescriptor ExcludeParentTypeIsComponentType = new(
              id: "MONO_BINDER_0004"
            , title: "[MonoBinderExcludeParent] type must not be the component type itself"
            , messageFormat: "[MonoBinderExcludeParent]: \"{0}\" is the same as the component type and has no effect."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [MonoBinderExcludeParent] must not be the same as the component type."
        );

        public static readonly DiagnosticDescriptor ExcludeParentTypeNotInHierarchy = new(
              id: "MONO_BINDER_0005"
            , title: "[MonoBinderExcludeParent] type must be a base type of the component type"
            , messageFormat: "[MonoBinderExcludeParent]: \"{0}\" is not a base type of component type \"{1}\"."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [MonoBinderExcludeParent] must be a base type (at any level) of the component type."
        );

        public static readonly DiagnosticDescriptor ObsoleteExplicitMemberWithExcludeObsolete = new(
              id: "MONO_BINDER_0006"
            , title: "[MonoBinder(ExcludeObsolete = true)] conflict with explicit binding attribute on obsolete member"
            , messageFormat: "[MonoBinder(ExcludeObsolete = true)]: \"{0}\" is marked [Obsolete] on \"{1}\". Remove [{2}] or set ExcludeObsolete = false."
            , category: "MonoBinderGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "When ExcludeObsolete = true, members marked [Obsolete] must not be explicitly included via [MonoBindingProperty] or [MonoBindingCommand]."
        );

        public static readonly DiagnosticDescriptor ExcludeObsoleteMemberRedundant = new(
              id: "MONO_BINDER_0007"
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

                var fullName = attrClass.ToDisplayString();

                if (fullName == MONO_BINDER_ATTRIBUTE)
                {
                    monoBinderAttr = attr;
                }
                else if (fullName == MONO_BINDING_PROP_ATTRIBUTE)
                {
                    hasMonoBindingProp = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr     = attr;
                        firstOrphanAttrName = "MonoBindingProperty";
                    }
                }
                else if (fullName == MONO_BINDING_CMD_ATTRIBUTE)
                {
                    hasMonoBindingCmd = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr     = attr;
                        firstOrphanAttrName = "MonoBindingCommand";
                    }
                }
                else if (fullName == MONO_BINDING_EXCLUDE_ATTRIBUTE)
                {
                    hasMonoBindingExclude = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr     = attr;
                        firstOrphanAttrName = "MonoBindingExclude";
                    }
                }
                else if (fullName == MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE)
                {
                    hasMonoBinderExcludeParent = true;

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr     = attr;
                        firstOrphanAttrName = "MonoBinderExcludeParent";
                    }
                }
            }

            // ── Diagnostic 2: binding/exclude attrs without [MonoBinder] ─────────────
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

                // No [MonoBinder] → remaining checks are meaningless
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

            // ── Diagnostic 1: component type must inherit UnityEngine.Object ──────────
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

            // ── Diagnostic 6 & 7: ExcludeObsolete validation ─────────────────────────
            if (excludeObsolete)
            {
                foreach (var attr in typeSymbol.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;

                    if (attrClass is null)
                    {
                        continue;
                    }

                    var fullName = attrClass.ToDisplayString();

                    if (fullName == MONO_BINDING_PROP_ATTRIBUTE || fullName == MONO_BINDING_CMD_ATTRIBUTE)
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
                            var attrName = fullName == MONO_BINDING_PROP_ATTRIBUTE
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
                    else if (fullName == MONO_BINDING_EXCLUDE_ATTRIBUTE)
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

            // ── Diagnostic 4 & 5: [MonoBinderExcludeParent] type validation ─────────
            if (hasMonoBinderExcludeParent)
            {
                foreach (var attr in typeSymbol.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;

                    if (attrClass is null || attrClass.ToDisplayString() != MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE)
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
                    else if (!IsInBaseTypeChain(componentType, parentType))
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

            // ── Diagnostic 3: [MonoBindingExclude] member not found on component ──────
            if (hasMonoBindingExclude == false)
            {
                return;
            }

            foreach (var attr in typeSymbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;

                if (attrClass is null || attrClass.ToDisplayString() != MONO_BINDING_EXCLUDE_ATTRIBUTE)
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

                if (!ComponentHasMember(componentType, excludedName))
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
                if (walk.ToDisplayString() == UNITY_OBJECT_TYPE)
                {
                    return true;
                }

                walk = walk.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="componentType"/> has
        /// <paramref name="candidate"/> anywhere in its base-type chain, stopping before
        /// <c>UnityEngine.Object</c>.
        /// </summary>
        private static bool IsInBaseTypeChain(INamedTypeSymbol componentType, INamedTypeSymbol candidate)
        {
            var walk = componentType.BaseType;

            while (walk != null)
            {
                if (walk.ToDisplayString() == UNITY_OBJECT_TYPE)
                {
                    break;
                }

                if (SymbolEqualityComparer.Default.Equals(walk, candidate))
                {
                    return true;
                }

                walk = walk.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="componentType"/> or any of its
        /// base types exposes a public non-static property, field, or event named
        /// <paramref name="memberName"/>.
        /// </summary>
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

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="componentType"/> or any of its
        /// base types exposes a public non-static property, field, or event named
        /// <paramref name="memberName"/> that carries <c>[System.Obsolete]</c>.
        /// </summary>
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
                            if (attr.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute")
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
