using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Entities.Lookups
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class LookupDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string LOOKUP_ATTRIBUTE = $"global::{NAMESPACE}.LookupAttribute";
        private const string I_BUFFER_LOOKUPS = $"{NAMESPACE}.IBufferLookups";
        private const string I_COMPONENT_LOOKUPS = $"{NAMESPACE}.IComponentLookups";
        private const string I_ENABLEABLE_BUFFER_LOOKUPS = $"{NAMESPACE}.IEnableableBufferLookups";
        private const string I_ENABLEABLE_COMPONENT_LOOKUPS = $"{NAMESPACE}.IEnableableComponentLookups";
        private const string I_PHYSICS_BUFFER_LOOKUPS = $"{NAMESPACE}.IPhysicsBufferLookups";
        private const string I_PHYSICS_COMPONENT_LOOKUPS = $"{NAMESPACE}.IPhysicsComponentLookups";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS = $"{NAMESPACE}.IPhysicsEnableableComponentLookups";
        private const string I_BUFFER_ELEMENT_DATA = "global::Unity.Entities.IBufferElementData";
        private const string I_COMPONENT_DATA = "global::Unity.Entities.IComponentData";
        private const string I_ENABLEABLE_COMPONENT = "global::Unity.Entities.IEnableableComponent";

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new(
              id: "SG_LOOKUPS_0001"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The first argument must be a 'typeof' expression."
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new(
              id: "SG_LOOKUPS_0002"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be open generic."
        );

        public static readonly DiagnosticDescriptor ManagedTypeNotSupported = new(
              id: "SG_LOOKUPS_0003"
            , title: "Type that is managed or contains managed reference is not supported"
            , messageFormat: "The type \"{0}\" must be unmanaged"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must be unmanaged."
        );

        public static readonly DiagnosticDescriptor IncompatInterface = new(
              id: "SG_LOOKUPS_0004"
            , title: "Type must implement compatible interface"
            , messageFormat: "The type \"{0}\" must implement interface \"{1}\" to be compatible"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must implement compatible interface."
        );

        public static readonly DiagnosticDescriptor NoMarkerInterface = new(
              id: "SG_LOOKUPS_0005"
            , title: "Missing lookup marker interface"
            , messageFormat: "Struct \"{0}\" must implement exactly one lookup marker interface"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The struct must implement exactly one of the lookup marker interfaces (e.g. IBufferLookups, IComponentLookups)."
        );

        public static readonly DiagnosticDescriptor MultipleMarkerInterfaces = new(
              id: "SG_LOOKUPS_0006"
            , title: "Multiple lookup marker interfaces are not supported"
            , messageFormat: "Struct \"{0}\" implements {1} lookup marker interfaces; only one is allowed"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The struct must implement exactly one lookup marker interface. Implementing two or more is not supported."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  NotTypeOfExpression
                , OpenGenericTypeNotSupported
                , ManagedTypeNotSupported
                , IncompatInterface
                , NoMarkerInterface
                , MultipleMarkerInterfaces
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
                || typeSymbol.HasAttribute(LOOKUP_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            var markerCount = CountMarkerInterfaces(typeSymbol, token);

            if (markerCount == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      NoMarkerInterface
                    , typeSymbol.Locations[0]
                    , typeSymbol.Name
                ));
                return;
            }

            if (markerCount >= 2)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MultipleMarkerInterfaces
                    , typeSymbol.Locations[0]
                    , typeSymbol.Name
                    , markerCount
                ));
                return;
            }

            GetRequiredEcsInterfaces(typeSymbol, out var interface1, out var interface2, token);

            foreach (var attrib in typeSymbol.GetAttributes(LOOKUP_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                var args = attrib.ConstructorArguments;
                var location = attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                    ?? typeSymbol.Locations[0];

                if (args.Length < 1
                    || args[0].Kind != TypedConstantKind.Type
                    || args[0].Value is not INamedTypeSymbol type
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          NotTypeOfExpression
                        , location
                    ));
                    continue;
                }

                if (type.IsUnboundGenericType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          OpenGenericTypeNotSupported
                        , location
                        , type.Name
                    ));
                    continue;
                }

                if (type.IsUnmanagedType == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          ManagedTypeNotSupported
                        , location
                        , type.Name
                    ));
                    continue;
                }

                if (interface1 != null && type.InheritsFromInterface(interface1, true, token) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          IncompatInterface
                        , location
                        , type.Name
                        , interface1.Remove(0, 8)
                    ));
                    continue;
                }

                if (interface2 != null && type.InheritsFromInterface(interface2, true, token) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          IncompatInterface
                        , location
                        , type.Name
                        , interface2.Remove(0, 8)
                    ));
                }
            }
        }

        private static int CountMarkerInterfaces(INamedTypeSymbol typeSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var count = 0;

            foreach (var iface in typeSymbol.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.HasFullName(I_BUFFER_LOOKUPS, token)
                    || iface.HasFullName(I_COMPONENT_LOOKUPS, token)
                    || iface.HasFullName(I_ENABLEABLE_BUFFER_LOOKUPS, token)
                    || iface.HasFullName(I_ENABLEABLE_COMPONENT_LOOKUPS, token)
                    || iface.HasFullName(I_PHYSICS_BUFFER_LOOKUPS, token)
                    || iface.HasFullName(I_PHYSICS_COMPONENT_LOOKUPS, token)
                    || iface.HasFullName(I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS, token)
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static void GetRequiredEcsInterfaces(
              INamedTypeSymbol typeSymbol
            , out string interface1
            , out string interface2
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            interface1 = null;
            interface2 = null;

            foreach (var iface in typeSymbol.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.HasFullName(I_BUFFER_LOOKUPS, token)
                    || iface.HasFullName(I_PHYSICS_BUFFER_LOOKUPS, token)
                )
                {
                    interface1 = I_BUFFER_ELEMENT_DATA;
                    return;
                }

                if (iface.HasFullName(I_COMPONENT_LOOKUPS, token)
                    || iface.HasFullName(I_PHYSICS_COMPONENT_LOOKUPS, token)
                )
                {
                    interface1 = I_COMPONENT_DATA;
                    return;
                }

                if (iface.HasFullName(I_ENABLEABLE_BUFFER_LOOKUPS, token))
                {
                    interface1 = I_BUFFER_ELEMENT_DATA;
                    interface2 = I_ENABLEABLE_COMPONENT;
                    return;
                }

                if (iface.HasFullName(I_ENABLEABLE_COMPONENT_LOOKUPS, token)
                    || iface.HasFullName(I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS, token)
                )
                {
                    interface1 = I_COMPONENT_DATA;
                    interface2 = I_ENABLEABLE_COMPONENT;
                    return;
                }
            }
        }
    }
}
