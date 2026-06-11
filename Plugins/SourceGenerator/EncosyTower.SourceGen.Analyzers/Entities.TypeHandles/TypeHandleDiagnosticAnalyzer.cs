using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Entities.TypeHandles
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class TypeHandleDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Entities";
        private const string TYPE_HANDLE_ATTRIBUTE = $"global::{NAMESPACE}.TypeHandleAttribute";
        private const string I_BUFFER_ELEMENT_DATA = "global::Unity.Entities.IBufferElementData";
        private const string I_COMPONENT_DATA = "global::Unity.Entities.IComponentData";
        private const string I_SHARED_COMPONENT_DATA = "global::Unity.Entities.ISharedComponentData";

        public static readonly DiagnosticDescriptor GenericContainerNotSupported = new(
              id: "SG_TYPE_HANDLES_0001"
            , title: "Generic container is not supported"
            , messageFormat: "Struct \"{0}\" must not be generic to use [TypeHandle]"
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Generic container is not supported."
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new(
              id: "SG_TYPE_HANDLES_0002"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Not a typeof expression."
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new(
              id: "SG_TYPE_HANDLES_0003"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Open generic type is not supported."
        );

        public static readonly DiagnosticDescriptor ManagedTypeNotSupported = new(
              id: "SG_TYPE_HANDLES_0004"
            , title: "Managed type is not supported"
            , messageFormat: "The type \"{0}\" must be unmanaged"
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Managed type is not supported."
        );

        public static readonly DiagnosticDescriptor MissingMarkerInterface = new(
              id: "SG_TYPE_HANDLES_0005"
            , title: "Missing ECS marker interface"
            , messageFormat: "The type \"{0}\" must implement IComponentData, IBufferElementData, or ISharedComponentData"
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Missing ECS marker interface."
        );

        public static readonly DiagnosticDescriptor DuplicateTypeHandle = new(
              id: "SG_TYPE_HANDLES_0006"
            , title: "Duplicate [TypeHandle] entry"
            , messageFormat: "The type \"{0}\" is already declared via another [TypeHandle] attribute on this struct; the duplicate is ignored"
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Duplicate [TypeHandle] entry."
        );

        public static readonly DiagnosticDescriptor ReadOnlyIgnoredForSharedComponent = new(
              id: "SG_TYPE_HANDLES_0007"
            , title: "isReadOnly is ignored for shared components"
            , messageFormat: "The 'isReadOnly' argument is ignored for ISharedComponentData type \"{0}\""
            , category: "TypeHandles"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "isReadOnly is ignored for shared components."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  GenericContainerNotSupported
                , NotTypeOfExpression
                , OpenGenericTypeNotSupported
                , ManagedTypeNotSupported
                , MissingMarkerInterface
                , DuplicateTypeHandle
                , ReadOnlyIgnoredForSharedComponent
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
                || typeSymbol.HasAttribute(TYPE_HANDLE_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            if (typeSymbol.IsGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      GenericContainerNotSupported
                    , typeSymbol.Locations[0]
                    , typeSymbol.Name
                ));
            }

            token.ThrowIfCancellationRequested();

            var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var attrib in typeSymbol.GetAttributes(TYPE_HANDLE_ATTRIBUTE, token))
            {
                token.ThrowIfCancellationRequested();

                var location = attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                    ?? typeSymbol.Locations[0];

                var args = attrib.ConstructorArguments;

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

                var kind = GetHandleKind(type, token);

                if (kind is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          MissingMarkerInterface
                        , location
                        , type.Name
                    ));
                    continue;
                }

                if (seen.Add(type) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          DuplicateTypeHandle
                        , location
                        , type.Name
                    ));
                }

                if (kind == HandleKind.SharedComponent
                    && args.Length > 1
                    && args[1].Value is true
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          ReadOnlyIgnoredForSharedComponent
                        , location
                        , type.Name
                    ));
                }
            }
        }

        private static HandleKind? GetHandleKind(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var iface in type.AllInterfaces)
            {
                token.ThrowIfCancellationRequested();

                if (iface.HasFullName(I_BUFFER_ELEMENT_DATA, token))
                    return HandleKind.Buffer;

                if (iface.HasFullName(I_COMPONENT_DATA, token))
                    return HandleKind.Component;

                if (iface.HasFullName(I_SHARED_COMPONENT_DATA, token))
                    return HandleKind.SharedComponent;
            }

            return null;
        }

        private enum HandleKind
        {
            Buffer,
            Component,
            SharedComponent,
        }
    }
}
