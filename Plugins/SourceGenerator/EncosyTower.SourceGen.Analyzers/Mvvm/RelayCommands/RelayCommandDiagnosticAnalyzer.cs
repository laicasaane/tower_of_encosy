using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Mvvm.RelayCommands
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class RelayCommandDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";
        private const string OBSERVABLE_OBJECT_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.ObservableObjectAttribute";
        private const string CAN_EXECUTE = "CanExecute";

        private const string CATEGORY = "EncosyTower.Mvvm.ComponentModel.ObservableObject";

        public static readonly DiagnosticDescriptor TooManyParameters = new(
              id: "SG_RELAY_COMMAND_0001"
            , title: "RelayCommand method has too many parameters"
            , messageFormat: "Method \"{0}\" has [RelayCommand] but takes {1} parameters; only 0 or 1 parameters are supported"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "RelayCommand method has too many parameters."
        );

        public static readonly DiagnosticDescriptor InvalidParameterRefKind = new(
              id: "SG_RELAY_COMMAND_0002"
            , title: "RelayCommand parameter must be by-value or 'in'"
            , messageFormat: "Method \"{0}\" has [RelayCommand] but its parameter uses ref-kind \"{1}\"; only by-value or 'in' are supported"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "RelayCommand parameter must be by-value or in."
        );

        public static readonly DiagnosticDescriptor MissingObservableObject = new(
              id: "SG_RELAY_COMMAND_0003"
            , title: "RelayCommand container must have [ObservableObject]"
            , messageFormat: "Class \"{0}\" has methods marked [RelayCommand] but is not decorated with [ObservableObject]; generation is skipped"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "RelayCommand container must have [ObservableObject]."
        );

        public static readonly DiagnosticDescriptor CanExecuteNotFound = new(
              id: "SG_RELAY_COMMAND_0004"
            , title: "CanExecute method not found"
            , messageFormat: "[RelayCommand(CanExecute = \"{0}\")] on method \"{1}\": no method named \"{0}\" found in class \"{2}\"; correlation ignored"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "CanExecute method not found."
        );

        public static readonly DiagnosticDescriptor CanExecuteWrongReturnType = new(
              id: "SG_RELAY_COMMAND_0005"
            , title: "CanExecute method must return bool"
            , messageFormat: "[RelayCommand(CanExecute = \"{0}\")] on method \"{1}\": referenced method \"{0}\" must return bool"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "CanExecute method must return bool."
        );

        public static readonly DiagnosticDescriptor CanExecuteParameterCountMismatch = new(
              id: "SG_RELAY_COMMAND_0006"
            , title: "CanExecute parameter count mismatch"
            , messageFormat: "[RelayCommand(CanExecute = \"{0}\")] on method \"{1}\": referenced method has {2} parameter(s) but command method has {3}"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "CanExecute parameter count mismatch."
        );

        public static readonly DiagnosticDescriptor CanExecuteParameterTypeMismatch = new(
              id: "SG_RELAY_COMMAND_0007"
            , title: "CanExecute parameter type mismatch"
            , messageFormat: "[RelayCommand(CanExecute = \"{0}\")] on method \"{1}\": parameter types of \"{0}\" are not a subset of the command method's parameter types"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "CanExecute parameter type mismatch."
        );

        public static readonly DiagnosticDescriptor StaticNotSupported = new(
              id: "SG_RELAY_COMMAND_0008"
            , title: "RelayCommand cannot be static"
            , messageFormat: "Method \"{0}\" cannot be [RelayCommand]; static methods are not supported"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "RelayCommand cannot be static."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  TooManyParameters
                , InvalidParameterRefKind
                , MissingObservableObject
                , CanExecuteNotFound
                , CanExecuteWrongReturnType
                , CanExecuteParameterCountMismatch
                , CanExecuteParameterTypeMismatch
                , StaticNotSupported
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

            BuildMethodDirectories(typeSymbol, out var allMethodMap, out var relayMethods, token);

            if (relayMethods.Count == 0)
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            var hasObservableObject = typeSymbol.HasAttribute(OBSERVABLE_OBJECT_ATTRIBUTE, token);
            var containerReported = false;

            foreach (var method in relayMethods)
            {
                token.ThrowIfCancellationRequested();

                var methodLocation = method.Locations.Length > 0
                    ? method.Locations[0]
                    : Location.None;

                var attrib = TryLocateRelayCommandAttribute(method, token);

                var attribLocation = attrib?.ApplicationSyntaxReference
                    ?.GetSyntax(context.CancellationToken)?.GetLocation() ?? methodLocation;

                if (hasObservableObject == false && containerReported == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          MissingObservableObject
                        , attribLocation
                        , typeSymbol.Name
                    ));
                    containerReported = true;
                }

                CheckStatic(context, method, methodLocation);
                CheckMethodArity(context, method, methodLocation);
                CheckParameterRefKind(context, method, methodLocation);
                CheckCanExecute(context, method, attrib, attribLocation, typeSymbol, allMethodMap);
            }
        }

        private static void BuildMethodDirectories(
              INamedTypeSymbol typeSymbol
            , out Dictionary<string, IMethodSymbol> allMethodMap
            , out List<IMethodSymbol> relayMethods
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            allMethodMap = new Dictionary<string, IMethodSymbol>(StringComparer.Ordinal);
            relayMethods = new List<IMethodSymbol>();

            foreach (var member in typeSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.MethodKind != MethodKind.Ordinary
                )
                {
                    continue;
                }

                allMethodMap[method.Name] = method;

                if (method.HasAttribute(RELAY_COMMAND_ATTRIBUTE, token))
                {
                    relayMethods.Add(method);
                }
            }
        }

        private static AttributeData TryLocateRelayCommandAttribute(IMethodSymbol method, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var attr in method.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attr.AttributeClass.HasFullName(RELAY_COMMAND_ATTRIBUTE, token))
                {
                    return attr;
                }
            }

            return null;
        }

        private static void CheckStatic(
              SymbolAnalysisContext context
            , IMethodSymbol method
            , Location methodLocation
        )
        {
            if (method.IsStatic == false)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                  StaticNotSupported
                , methodLocation
                , method.Name
            ));
        }

        private static void CheckMethodArity(
              SymbolAnalysisContext context
            , IMethodSymbol method
            , Location methodLocation
        )
        {
            if (method.Parameters.Length <= 1)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                  TooManyParameters
                , methodLocation
                , method.Name
                , method.Parameters.Length
            ));
        }

        private static void CheckParameterRefKind(
              SymbolAnalysisContext context
            , IMethodSymbol method
            , Location methodLocation
        )
        {
            if (method.Parameters.Length != 1)
            {
                return;
            }

            var refKind = method.Parameters[0].RefKind;

            if (refKind == RefKind.None || refKind == RefKind.In)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                  InvalidParameterRefKind
                , methodLocation
                , method.Name
                , refKind.ToString()
            ));
        }

        private static void CheckCanExecute(
              SymbolAnalysisContext context
            , IMethodSymbol method
            , AttributeData attrib
            , Location attribLocation
            , INamedTypeSymbol typeSymbol
            , Dictionary<string, IMethodSymbol> allMethodMap
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (attrib is null || attrib.NamedArguments.Length == 0)
            {
                return;
            }

            string canExecuteName = null;

            foreach (var kv in attrib.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(kv.Key, CAN_EXECUTE, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                var value = kv.Value;

                if (value.IsNull
                    || value.Kind != TypedConstantKind.Primitive
                    || value.Value is not string stringValue
                    || string.IsNullOrEmpty(stringValue)
                )
                {
                    return;
                }

                canExecuteName = stringValue;
                break;
            }

            if (canExecuteName is null)
            {
                return;
            }

            if (allMethodMap.TryGetValue(canExecuteName, out var canExecuteMethod) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      CanExecuteNotFound
                    , attribLocation
                    , canExecuteName
                    , method.Name
                    , typeSymbol.Name
                ));
                return;
            }

            if (canExecuteMethod.ReturnType.SpecialType != SpecialType.System_Boolean)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      CanExecuteWrongReturnType
                    , attribLocation
                    , canExecuteName
                    , method.Name
                ));
                return;
            }

            if (canExecuteMethod.Parameters.Length == 0)
            {
                return;
            }

            if (canExecuteMethod.Parameters.Length != method.Parameters.Length)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      CanExecuteParameterCountMismatch
                    , attribLocation
                    , canExecuteName
                    , method.Name
                    , canExecuteMethod.Parameters.Length
                    , method.Parameters.Length
                ));
                return;
            }

            token.ThrowIfCancellationRequested();

            var commandParamTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var param in method.Parameters)
            {
                token.ThrowIfCancellationRequested();

                commandParamTypes.Add(param.Type);
            }

            token.ThrowIfCancellationRequested();

            foreach (var param in canExecuteMethod.Parameters)
            {
                token.ThrowIfCancellationRequested();

                if (commandParamTypes.Contains(param.Type) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          CanExecuteParameterTypeMismatch
                        , attribLocation
                        , canExecuteName
                        , method.Name
                    ));
                    return;
                }
            }
        }
    }
}
