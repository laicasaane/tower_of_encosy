using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Mvvm.ObservableProperties
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ObservablePropertyDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string OBSERVABLE_OBJECT_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.ObservableObjectAttribute";
        private const string OBSERVABLE_PROPERTY_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.ObservablePropertyAttribute";
        private const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedForAttribute";
        private const string NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.NotifyCanExecuteChangedForAttribute";
        private const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";

        private const string CATEGORY = "ObservableProperties";

        private const string KIND_NOTIFY_PROPERTY_CHANGED_FOR = "NotifyPropertyChangedFor";
        private const string KIND_NOTIFY_CAN_EXECUTE_CHANGED_FOR = "NotifyCanExecuteChangedFor";
        private const string COMMAND_SUFFIX = "Command";

        public static readonly DiagnosticDescriptor MissingObservableObject = new(
              id: "SG_OBSERVABLE_PROPERTY_0001"
            , title: "Missing [ObservableObject] on containing class"
            , messageFormat: "Member \"{0}\" has [ObservableProperty] but containing class \"{1}\" is not decorated with [ObservableObject]; generation is skipped"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Missing [ObservableObject] on containing class."
        );

        public static readonly DiagnosticDescriptor NotifyPropertyChangedForTargetMissing = new(
              id: "SG_OBSERVABLE_PROPERTY_0002"
            , title: "[NotifyPropertyChangedFor] target property not found"
            , messageFormat: "[NotifyPropertyChangedFor(\"{0}\")] on member \"{1}\": no property named \"{0}\" found in class \"{2}\"; correlation ignored"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "[NotifyPropertyChangedFor] target property not found."
        );

        public static readonly DiagnosticDescriptor NotifyCanExecuteChangedForTargetMissing = new(
              id: "SG_OBSERVABLE_PROPERTY_0003"
            , title: "[NotifyCanExecuteChangedFor] target command not found"
            , messageFormat: "[NotifyCanExecuteChangedFor(\"{0}\")] on member \"{1}\": no method \"{2}\" decorated with [RelayCommand] found in class \"{3}\"; correlation ignored"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "[NotifyCanExecuteChangedFor] target command not found."
        );

        public static readonly DiagnosticDescriptor StaticMemberNotSupported = new(
              id: "SG_OBSERVABLE_PROPERTY_0004"
            , title: "[ObservableProperty] on static member is not supported"
            , messageFormat: "Member \"{0}\" cannot be [ObservableProperty]; static members are not supported"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[ObservableProperty] on static member is not supported."
        );

        public static readonly DiagnosticDescriptor ReadOnlyFieldNotSupported = new(
              id: "SG_OBSERVABLE_PROPERTY_0005"
            , title: "[ObservableProperty] on readonly field is not supported"
            , messageFormat: "Field \"{0}\" cannot be [ObservableProperty]; readonly fields are not supported"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[ObservableProperty] on readonly field is not supported."
        );

        public static readonly DiagnosticDescriptor DuplicateGeneratedPropertyName = new(
              id: "SG_OBSERVABLE_PROPERTY_0006"
            , title: "Duplicate generated property name"
            , messageFormat: "Member \"{0}\" would generate property \"{1}\" but that name is already used by member \"{2}\""
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Duplicate generated property name."
        );

        public static readonly DiagnosticDescriptor CorrelationArgNotStringConstant = new(
              id: "SG_OBSERVABLE_PROPERTY_0007"
            , title: "Correlation argument is not a string constant"
            , messageFormat: "[{0}] on member \"{1}\": argument is not a string constant; correlation ignored"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Correlation argument is not a string constant."
        );

        public static readonly DiagnosticDescriptor CorrelationArgEmpty = new(
              id: "SG_OBSERVABLE_PROPERTY_0008"
            , title: "Correlation attribute has no targets"
            , messageFormat: "[{0}] on member \"{1}\" has no target names; attribute is ignored"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Correlation attribute has no targets."
        );

        public static readonly DiagnosticDescriptor InvalidCustomPropertyName = new(
              id: "SG_OBSERVABLE_PROPERTY_0009"
            , title: "[ObservableProperty] custom name is not a valid C# identifier"
            , messageFormat: "[ObservableProperty(\"{0}\")] on member \"{1}\": \"{0}\" is not a valid C# identifier"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[ObservableProperty] custom name is not a valid C# identifier."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MissingObservableObject
                , NotifyPropertyChangedForTargetMissing
                , NotifyCanExecuteChangedForTargetMissing
                , StaticMemberNotSupported
                , ReadOnlyFieldNotSupported
                , DuplicateGeneratedPropertyName
                , CorrelationArgNotStringConstant
                , CorrelationArgEmpty
                , InvalidCustomPropertyName
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.TypeKind != TypeKind.Class
            )
            {
                return;
            }

            var hasObservableObject = typeSymbol.HasAttribute(OBSERVABLE_OBJECT_ATTRIBUTE, token);

            BuildMemberDirectories(
                  typeSymbol
                , out var declaredMemberNames
                , out var declaredPropertyNames
                , out var relayCommandNames
                , out var observablePropertyMembers
                , token
            );

            if (hasObservableObject == false && observablePropertyMembers.Count == 0)
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            var generatedNames = new Dictionary<string, ISymbol>(StringComparer.Ordinal);

            foreach (var member in observablePropertyMembers)
            {
                token.ThrowIfCancellationRequested();

                var memberLocation = member.Locations.Length > 0
                    ? member.Locations[0]
                    : Location.None;

                if (hasObservableObject == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          MissingObservableObject
                        , memberLocation
                        , member.Name
                        , typeSymbol.Name
                    ));
                }

                if (member.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          StaticMemberNotSupported
                        , memberLocation
                        , member.Name
                    ));
                    continue;
                }

                if (member is IFieldSymbol fieldSymbol && fieldSymbol.IsReadOnly)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          ReadOnlyFieldNotSupported
                        , memberLocation
                        , fieldSymbol.Name
                    ));
                    continue;
                }

                var observableAttr = TryLocateObservableAttribute(member, token);

                var observableAttrLocation = observableAttr?.ApplicationSyntaxReference
                    ?.GetSyntax(context.CancellationToken)?.GetLocation() ?? memberLocation;

                if (TryComputeGeneratedName(
                      context
                    , member
                    , observableAttr
                    , observableAttrLocation
                    , out var computedName
                ) == false)
                {
                    continue;
                }

                if (TryReportDuplicateName(
                      context
                    , member
                    , computedName
                    , memberLocation
                    , declaredMemberNames
                    , generatedNames
                ))
                {
                    continue;
                }

                generatedNames[computedName] = member;

                AnalyzeCorrelationAttributes(
                      context
                    , member
                    , typeSymbol
                    , memberLocation
                    , declaredPropertyNames
                    , relayCommandNames
                );
            }
        }

        private static void BuildMemberDirectories(
              INamedTypeSymbol typeSymbol
            , out HashSet<string> declaredMemberNames
            , out HashSet<string> declaredPropertyNames
            , out HashSet<string> relayCommandNames
            , out List<ISymbol> observablePropertyMembers
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            declaredMemberNames = new HashSet<string>(StringComparer.Ordinal);
            declaredPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            relayCommandNames = new HashSet<string>(StringComparer.Ordinal);
            observablePropertyMembers = new List<ISymbol>();

            foreach (var member in typeSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                declaredMemberNames.Add(member.Name);

                if (member is IPropertySymbol propertySymbol)
                {
                    declaredPropertyNames.Add(propertySymbol.Name);
                }

                if (member is IMethodSymbol methodSymbol
                    && methodSymbol.HasAttribute(RELAY_COMMAND_ATTRIBUTE, token)
                )
                {
                    relayCommandNames.Add($"{methodSymbol.Name}{COMMAND_SUFFIX}");
                }

                if ((member is IFieldSymbol || member is IPropertySymbol)
                    && member.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE, token)
                )
                {
                    observablePropertyMembers.Add(member);
                }
            }
        }

        private static AttributeData TryLocateObservableAttribute(ISymbol member, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var attr in member.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attr.AttributeClass.HasFullName(OBSERVABLE_PROPERTY_ATTRIBUTE, token))
                {
                    return attr;
                }
            }

            return null;
        }

        private static bool TryComputeGeneratedName(
              SymbolAnalysisContext context
            , ISymbol member
            , AttributeData observableAttr
            , Location observableAttrLocation
            , out string computedName
        )
        {
            computedName = null;
            var hasCustomName = false;

            if (observableAttr is { ConstructorArguments.Length: >= 1 }
                && observableAttr.ConstructorArguments[0].Value is string customName
                && string.IsNullOrEmpty(customName) == false
            )
            {
                hasCustomName = true;

                if (SyntaxFacts.IsValidIdentifier(customName) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          InvalidCustomPropertyName
                        , observableAttrLocation
                        , customName
                        , member.Name
                    ));
                    return false;
                }

                computedName = customName;
            }

            if (hasCustomName == false)
            {
                computedName = DerivePropertyName(member.Name);

                if (string.IsNullOrEmpty(computedName)
                    || SyntaxFacts.IsValidIdentifier(computedName) == false
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          InvalidCustomPropertyName
                        , observableAttrLocation
                        , member.Name
                        , member.Name
                    ));
                    return false;
                }
            }

            return true;
        }

        private static bool TryReportDuplicateName(
              SymbolAnalysisContext context
            , ISymbol member
            , string computedName
            , Location memberLocation
            , HashSet<string> declaredMemberNames
            , Dictionary<string, ISymbol> generatedNames
        )
        {
            if (generatedNames.TryGetValue(computedName, out var prior))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      DuplicateGeneratedPropertyName
                    , memberLocation
                    , member.Name
                    , computedName
                    , prior.Name
                ));
                return true;
            }

            if (declaredMemberNames.Contains(computedName)
                && string.Equals(member.Name, computedName, StringComparison.Ordinal) == false
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      DuplicateGeneratedPropertyName
                    , memberLocation
                    , member.Name
                    , computedName
                    , computedName
                ));
                return true;
            }

            return false;
        }

        private static void AnalyzeCorrelationAttributes(
              SymbolAnalysisContext context
            , ISymbol member
            , INamedTypeSymbol typeSymbol
            , Location memberLocation
            , HashSet<string> declaredPropertyNames
            , HashSet<string> relayCommandNames
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (var attrib in member.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attrClass = attrib.AttributeClass;

                if (attrClass is null)
                {
                    continue;
                }

                string correlationKind;
                bool isPropertyChanged;

                if (attrClass.HasFullName(NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE, token))
                {
                    correlationKind = KIND_NOTIFY_PROPERTY_CHANGED_FOR;
                    isPropertyChanged = true;
                }
                else if (attrClass.HasFullName(NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE, token))
                {
                    correlationKind = KIND_NOTIFY_CAN_EXECUTE_CHANGED_FOR;
                    isPropertyChanged = false;
                }
                else
                {
                    continue;
                }

                var attribLocation = attrib.ApplicationSyntaxReference
                    ?.GetSyntax(context.CancellationToken)?.GetLocation() ?? memberLocation;

                var args = attrib.ConstructorArguments;

                if (args.Length == 0 || (args.Length == 1 && args[0].IsNull))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          CorrelationArgEmpty
                        , attribLocation
                        , correlationKind
                        , member.Name
                    ));
                    continue;
                }

                if (args[0].Kind != TypedConstantKind.Primitive || args[0].Value is not string firstName)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          CorrelationArgNotStringConstant
                        , attribLocation
                        , correlationKind
                        , member.Name
                    ));
                    continue;
                }

                var targetNames = new List<string> { firstName };
                var malformed = false;

                if (args.Length > 1 && args[1].Kind == TypedConstantKind.Array)
                {
                    foreach (var element in args[1].Values)
                    {
                        token.ThrowIfCancellationRequested();

                        if (element.Kind != TypedConstantKind.Primitive || element.Value is not string extraName)
                        {
                            malformed = true;
                            break;
                        }

                        targetNames.Add(extraName);
                    }
                }

                if (malformed)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          CorrelationArgNotStringConstant
                        , attribLocation
                        , correlationKind
                        , member.Name
                    ));
                    continue;
                }

                token.ThrowIfCancellationRequested();

                foreach (var targetName in targetNames)
                {
                    token.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(targetName))
                    {
                        continue;
                    }

                    if (isPropertyChanged)
                    {
                        if (declaredPropertyNames.Contains(targetName) == false)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  NotifyPropertyChangedForTargetMissing
                                , attribLocation
                                , targetName
                                , member.Name
                                , typeSymbol.Name
                            ));
                        }
                    }
                    else
                    {
                        if (relayCommandNames.Contains(targetName) == false)
                        {
                            var expectedMethodName = targetName.EndsWith(COMMAND_SUFFIX, StringComparison.Ordinal)
                                ? targetName.Substring(0, targetName.Length - COMMAND_SUFFIX.Length)
                                : targetName;

                            context.ReportDiagnostic(Diagnostic.Create(
                                  NotifyCanExecuteChangedForTargetMissing
                                , attribLocation
                                , targetName
                                , member.Name
                                , expectedMethodName
                                , typeSymbol.Name
                            ));
                        }
                    }
                }
            }
        }

        private static string DerivePropertyName(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                return memberName;
            }

            string body;

            if (memberName.StartsWith("m_", StringComparison.Ordinal) && memberName.Length > 2)
            {
                body = memberName.Substring(2);
            }
            else if (memberName[0] == '_' && memberName.Length > 1)
            {
                body = memberName.Substring(1);
            }
            else
            {
                body = memberName;
            }

            if (string.IsNullOrEmpty(body))
            {
                return body;
            }

            var first = body[0];

            if (char.IsUpper(first))
            {
                return body;
            }

            return char.ToUpperInvariant(first) + body.Substring(1);
        }
    }
}
