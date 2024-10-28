using Microsoft.CodeAnalysis;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.Modules.Entities.SourceGen
{
    internal static class LookupDiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor NotTypeOfExpression = new DiagnosticDescriptor(
              id: "LOOKUPS_0001"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The first argument must be a 'typeof' expression."
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new DiagnosticDescriptor(
              id: "LOOKUPS_0002"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must not be open generic."
        );

        public static readonly DiagnosticDescriptor ManagedTypeNotSupported = new DiagnosticDescriptor(
              id: "LOOKUPS_0003"
            , title: "Type that is managed or contains managed reference is not supported"
            , messageFormat: "The type \"{0}\" must be unmanaged"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must be unmanaged."
        );

        public static readonly DiagnosticDescriptor IncompatInterface = new DiagnosticDescriptor(
              id: "LOOKUPS_0003"
            , title: "Type must implement compatible interface"
            , messageFormat: "The type \"{0}\" must implement interface \"{1}\" to be compatible"
            , category: "Lookups"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type must implement compatible interface."
        );
    }
}
