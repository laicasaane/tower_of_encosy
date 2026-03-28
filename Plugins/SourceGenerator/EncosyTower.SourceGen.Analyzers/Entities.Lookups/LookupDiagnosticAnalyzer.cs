using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Analyzers.Entities.Lookups
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for structs annotated with
    /// <c>[Lookup]</c>, covering conditions that would cause the source generator
    /// to silently produce no output or invalid output.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class LookupDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string LOOKUP_ATTRIBUTE = $"global::{NAMESPACE}.LookupAttribute";

        // Marker interfaces — used to determine required ECS interfaces per lookup kind.
        // Compared against iface.ToDisplayString() (which omits the "global::" prefix).
        private const string I_BUFFER_LOOKUPS                       = $"{NAMESPACE}.IBufferLookups";
        private const string I_COMPONENT_LOOKUPS                    = $"{NAMESPACE}.IComponentLookups";
        private const string I_ENABLEABLE_BUFFER_LOOKUPS            = $"{NAMESPACE}.IEnableableBufferLookups";
        private const string I_ENABLEABLE_COMPONENT_LOOKUPS         = $"{NAMESPACE}.IEnableableComponentLookups";
        private const string I_PHYSICS_BUFFER_LOOKUPS               = $"{NAMESPACE}.IPhysicsBufferLookups";
        private const string I_PHYSICS_COMPONENT_LOOKUPS            = $"{NAMESPACE}.IPhysicsComponentLookups";
        private const string I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS = $"{NAMESPACE}.IPhysicsEnableableComponentLookups";

        // ECS interfaces (with "global::" prefix — as expected by InheritsFromInterface)
        private const string I_BUFFER_ELEMENT_DATA  = "global::Unity.Entities.IBufferElementData";
        private const string I_COMPONENT_DATA       = "global::Unity.Entities.IComponentData";
        private const string I_ENABLEABLE_COMPONENT = "global::Unity.Entities.IEnableableComponent";

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new(
              id: "LOOKUPS_0001"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The first argument must be a 'typeof' expression."
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new(
              id: "LOOKUPS_0002"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be open generic."
        );

        public static readonly DiagnosticDescriptor ManagedTypeNotSupported = new(
              id: "LOOKUPS_0003"
            , title: "Type that is managed or contains managed reference is not supported"
            , messageFormat: "The type \"{0}\" must be unmanaged"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must be unmanaged."
        );

        public static readonly DiagnosticDescriptor IncompatInterface = new(
              id: "LOOKUPS_0004"
            , title: "Type must implement compatible interface"
            , messageFormat: "The type \"{0}\" must implement interface \"{1}\" to be compatible"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must implement compatible interface."
        );

        public static readonly DiagnosticDescriptor NoMarkerInterface = new(
              id: "LOOKUPS_0005"
            , title: "Missing lookup marker interface"
            , messageFormat: "Struct \"{0}\" must implement exactly one lookup marker interface"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The struct must implement exactly one of the lookup marker interfaces (e.g. IBufferLookups, IComponentLookups)."
        );

        public static readonly DiagnosticDescriptor MultipleMarkerInterfaces = new(
              id: "LOOKUPS_0006"
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
            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(LOOKUP_ATTRIBUTE) == false
            )
            {
                return;
            }

            var markerCount = CountMarkerInterfaces(typeSymbol);

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

            GetRequiredEcsInterfaces(typeSymbol, out var interface1, out var interface2);

            foreach (var attrib in typeSymbol.GetAttributes(LOOKUP_ATTRIBUTE))
            {
                var args = attrib.ConstructorArguments;
                var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
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

                if (interface1 != null && type.InheritsFromInterface(interface1) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          IncompatInterface
                        , location
                        , type.Name
                        , interface1.Remove(0, 8)
                    ));
                    continue;
                }

                if (interface2 != null && type.InheritsFromInterface(interface2) == false)
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

        private static int CountMarkerInterfaces(INamedTypeSymbol typeSymbol)
        {
            var count = 0;

            foreach (var iface in typeSymbol.AllInterfaces)
            {
                if (iface.HasFullName(I_BUFFER_LOOKUPS)
                    || iface.HasFullName(I_COMPONENT_LOOKUPS)
                    || iface.HasFullName(I_ENABLEABLE_BUFFER_LOOKUPS)
                    || iface.HasFullName(I_ENABLEABLE_COMPONENT_LOOKUPS)
                    || iface.HasFullName(I_PHYSICS_BUFFER_LOOKUPS)
                    || iface.HasFullName(I_PHYSICS_COMPONENT_LOOKUPS)
                    || iface.HasFullName(I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS)
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
        )
        {
            interface1 = null;
            interface2 = null;

            foreach (var iface in typeSymbol.AllInterfaces)
            {
                if (iface.HasFullName(I_BUFFER_LOOKUPS)
                    || iface.HasFullName(I_PHYSICS_BUFFER_LOOKUPS)
                )
                {
                    interface1 = I_BUFFER_ELEMENT_DATA;
                    return;
                }

                if (iface.HasFullName(I_COMPONENT_LOOKUPS)
                    || iface.HasFullName(I_PHYSICS_COMPONENT_LOOKUPS)
                )
                {
                    interface1 = I_COMPONENT_DATA;
                    return;
                }

                if (iface.HasFullName(I_ENABLEABLE_BUFFER_LOOKUPS))
                {
                    interface1 = I_BUFFER_ELEMENT_DATA;
                    interface2 = I_ENABLEABLE_COMPONENT;
                    return;
                }

                if (iface.HasFullName(I_ENABLEABLE_COMPONENT_LOOKUPS)
                    || iface.HasFullName(I_PHYSICS_ENABLEABLE_COMPONENT_LOOKUPS)
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
