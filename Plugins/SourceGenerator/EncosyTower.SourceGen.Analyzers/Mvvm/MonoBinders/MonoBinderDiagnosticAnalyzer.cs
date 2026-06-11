using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
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
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            BuildAttributeDirectories(
                  typeSymbol
                , out var monoBinderAttr
                , out var bindingExcludeAttrs
                , out var binderExcludeParentAttrs
                , out var firstOrphanAttr
                , out var firstOrphanAttrName
                , token
            );

            if (TryReportOrphanBindingAttribute(
                  context
                , typeSymbol
                , monoBinderAttr
                , firstOrphanAttr
                , firstOrphanAttrName
            ))
            {
                return;
            }

            if (monoBinderAttr == null
                || TryGetComponentType(monoBinderAttr, out var componentType) == false
            )
            {
                return;
            }

            var excludeObsolete = ReadExcludeObsoleteFlag(monoBinderAttr, token);

            AnalyzeComponentType(context, typeSymbol, componentType, monoBinderAttr);

            if (excludeObsolete)
            {
                AnalyzeObsoleteConflicts(context, typeSymbol, componentType);
            }

            if (binderExcludeParentAttrs is { Count: > 0 })
            {
                AnalyzeExcludeParent(context, typeSymbol, componentType, binderExcludeParentAttrs);
            }

            if (bindingExcludeAttrs is { Count: > 0 })
            {
                AnalyzeExcludedMembers(context, typeSymbol, componentType, bindingExcludeAttrs);
            }
        }

        private static void BuildAttributeDirectories(
              INamedTypeSymbol typeSymbol
            , out AttributeData monoBinderAttr
            , out List<AttributeData> bindingExcludeAttrs
            , out List<AttributeData> binderExcludeParentAttrs
            , out AttributeData firstOrphanAttr
            , out string firstOrphanAttrName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            monoBinderAttr = null;
            bindingExcludeAttrs = null;
            binderExcludeParentAttrs = null;
            firstOrphanAttr = null;
            firstOrphanAttrName = null;

            foreach (var attr in typeSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attrClass = attr.AttributeClass;

                if (attrClass is null)
                {
                    continue;
                }

                if (attrClass.IsType(MONO_BINDER_ATTRIBUTE, token))
                {
                    monoBinderAttr = attr;
                }
                else if (attrClass.IsType(MONO_BINDING_PROP_ATTRIBUTE, token))
                {
                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBindingProperty";
                    }
                }
                else if (attrClass.IsType(MONO_BINDING_CMD_ATTRIBUTE, token))
                {
                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBindingCommand";
                    }
                }
                else if (attrClass.IsType(MONO_BINDING_EXCLUDE_ATTRIBUTE, token))
                {
                    (bindingExcludeAttrs ??= new List<AttributeData>()).Add(attr);

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBindingExclude";
                    }
                }
                else if (attrClass.IsType(MONO_BINDER_EXCLUDE_PARENT_ATTRIBUTE, token))
                {
                    (binderExcludeParentAttrs ??= new List<AttributeData>()).Add(attr);

                    if (firstOrphanAttr == null)
                    {
                        firstOrphanAttr = attr;
                        firstOrphanAttrName = "MonoBinderExcludeParent";
                    }
                }
            }
        }

        private static bool TryReportOrphanBindingAttribute(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , AttributeData monoBinderAttr
            , AttributeData firstOrphanAttr
            , string firstOrphanAttrName
        )
        {
            if (monoBinderAttr != null || firstOrphanAttr == null)
            {
                return false;
            }

            var location = firstOrphanAttr.ApplicationSyntaxReference
                ?.GetSyntax(context.CancellationToken)?.GetLocation()?? typeSymbol.Locations[0];

            context.ReportDiagnostic(Diagnostic.Create(
                  BindingAttributeRequiresMonoBinder
                , location
                , typeSymbol.Name
                , firstOrphanAttrName
            ));

            return true;
        }

        private static bool TryGetComponentType(AttributeData monoBinderAttr, out INamedTypeSymbol componentType)
        {
            if (monoBinderAttr.ConstructorArguments.Length < 1
                || monoBinderAttr.ConstructorArguments[0].Value is not INamedTypeSymbol named
            )
            {
                componentType = null;
                return false;
            }

            componentType = named;
            return true;
        }

        private static bool ReadExcludeObsoleteFlag(AttributeData monoBinderAttr, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var namedArg in monoBinderAttr.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(namedArg.Key, "ExcludeObsolete", StringComparison.Ordinal)
                    && namedArg.Value.Value is bool boolValue
                )
                {
                    return boolValue;
                }
            }

            return false;
        }

        private static void AnalyzeComponentType(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , INamedTypeSymbol componentType
            , AttributeData monoBinderAttr
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (InheritsFromUnityObject(componentType, token))
            {
                return;
            }

            var location = monoBinderAttr.ApplicationSyntaxReference
                ?.GetSyntax(token)?.GetLocation() ?? typeSymbol.Locations[0];

            context.ReportDiagnostic(Diagnostic.Create(
                  ComponentTypeMustInheritUnityObject
                , location
                , componentType.ToDisplayString()
            ));
        }

        private static void AnalyzeObsoleteConflicts(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , INamedTypeSymbol componentType
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (var attr in typeSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attrClass = attr.AttributeClass;

                if (attrClass is null)
                {
                    continue;
                }

                if (attrClass.IsType(MONO_BINDING_PROP_ATTRIBUTE, token))
                {
                    TryReportObsoleteExplicit(context, typeSymbol, componentType, attr, "MonoBindingProperty");
                }
                else if (attrClass.IsType(MONO_BINDING_CMD_ATTRIBUTE, token))
                {
                    TryReportObsoleteExplicit(context, typeSymbol, componentType, attr, "MonoBindingCommand");
                }
                else if (attrClass.IsType(MONO_BINDING_EXCLUDE_ATTRIBUTE, token))
                {
                    TryReportObsoleteRedundantExclude(context, typeSymbol, componentType, attr);
                }
            }
        }

        private static void TryReportObsoleteExplicit(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , INamedTypeSymbol componentType
            , AttributeData attr
            , string attrName
        )
        {
            if (attr.ConstructorArguments.Length < 1
                || attr.ConstructorArguments[0].Value is not string memberName
                || string.IsNullOrEmpty(memberName)
            )
            {
                return;
            }

            if (IsObsoleteMember(componentType, memberName, context.CancellationToken) == false)
            {
                return;
            }

            var location = attr.ApplicationSyntaxReference
                ?.GetSyntax(context.CancellationToken)?.GetLocation() ?? typeSymbol.Locations[0];

            context.ReportDiagnostic(Diagnostic.Create(
                  ObsoleteExplicitMemberWithExcludeObsolete
                , location
                , memberName
                , componentType.ToDisplayString()
                , attrName
            ));
        }

        private static void TryReportObsoleteRedundantExclude(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , INamedTypeSymbol componentType
            , AttributeData attr
        )
        {
            if (attr.ConstructorArguments.Length < 1
                || attr.ConstructorArguments[0].Value is not string memberName
                || string.IsNullOrEmpty(memberName)
            )
            {
                return;
            }

            if (IsObsoleteMember(componentType, memberName, context.CancellationToken) == false)
            {
                return;
            }

            var location = attr.ApplicationSyntaxReference
                ?.GetSyntax(context.CancellationToken)?.GetLocation() ?? typeSymbol.Locations[0];

            context.ReportDiagnostic(Diagnostic.Create(
                  ExcludeObsoleteMemberRedundant
                , location
                , memberName
            ));
        }

        private static void AnalyzeExcludeParent(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , INamedTypeSymbol componentType
            , List<AttributeData> binderExcludeParentAttrs
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (var attr in binderExcludeParentAttrs)
            {
                token.ThrowIfCancellationRequested();

                if (attr.ConstructorArguments.Length < 1
                    || attr.ConstructorArguments[0].Value is not INamedTypeSymbol parentType
                )
                {
                    continue;
                }

                var location = attr.ApplicationSyntaxReference
                    ?.GetSyntax(token)?.GetLocation() ?? typeSymbol.Locations[0];

                if (SymbolEqualityComparer.Default.Equals(parentType, componentType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          ExcludeParentTypeIsComponentType
                        , location
                        , parentType.ToDisplayString()
                    ));
                }
                else if (IsInBaseTypeChain(componentType, parentType, token) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          ExcludeParentTypeNotInHierarchy
                        , location
                        , parentType.ToDisplayString()
                        , componentType.ToDisplayString()
                    ));
                }
            }
        }

        private static void AnalyzeExcludedMembers(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , INamedTypeSymbol componentType
            , List<AttributeData> bindingExcludeAttrs
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (var attr in bindingExcludeAttrs)
            {
                token.ThrowIfCancellationRequested();

                if (attr.ConstructorArguments.Length < 1
                    || attr.ConstructorArguments[0].Value is not string excludedName
                    || string.IsNullOrEmpty(excludedName)
                )
                {
                    continue;
                }

                if (ComponentHasMember(componentType, excludedName, token))
                {
                    continue;
                }

                var location = attr.ApplicationSyntaxReference
                    ?.GetSyntax(token)?.GetLocation() ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      ExcludedMemberNotFound
                    , location
                    , excludedName
                    , componentType.ToDisplayString()
                ));
            }
        }

        private static bool InheritsFromUnityObject(ITypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var walk = type;

            while (walk != null)
            {
                token.ThrowIfCancellationRequested();

                if (walk.IsType(UNITY_OBJECT_TYPE, token))
                {
                    return true;
                }

                walk = walk.BaseType;
            }

            return false;
        }

        private static bool IsInBaseTypeChain(
              INamedTypeSymbol componentType
            , INamedTypeSymbol parentType
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var walk = componentType.BaseType;

            while (walk != null)
            {
                token.ThrowIfCancellationRequested();

                if (walk.IsType(UNITY_OBJECT_TYPE, token))
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

        private static bool ComponentHasMember(
              INamedTypeSymbol componentType
            , string memberName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var current = componentType as ITypeSymbol;

            while (current != null)
            {
                token.ThrowIfCancellationRequested();

                foreach (var member in current.GetMembers(memberName))
                {
                    token.ThrowIfCancellationRequested();

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

        private static bool IsObsoleteMember(
              INamedTypeSymbol componentType
            , string memberName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var current = componentType as ITypeSymbol;

            while (current != null)
            {
                token.ThrowIfCancellationRequested();

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
                            token.ThrowIfCancellationRequested();

                            if (attr.AttributeClass is { } attrib && attrib.IsType(OBSOLETE_ATTRIBUTE, token))
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
