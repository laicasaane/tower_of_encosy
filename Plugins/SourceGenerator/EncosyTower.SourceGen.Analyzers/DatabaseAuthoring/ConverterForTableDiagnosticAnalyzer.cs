using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking
#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace EncosyTower.SourceGen.Analyzers.DatabaseAuthoring
{
    /// <summary>
    /// Reports <see cref="MisplacedConverterAttribute"/> when a <c>ConverterForTableAttribute</c>
    /// is placed on a type that is not annotated with <c>AuthorDatabaseAttribute</c>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConverterForTableDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string AUTHOR_DATABASE_ATTRIBUTE = "global::EncosyTower.Databases.Authoring.AuthorDatabaseAttribute";
        private const string CONVERTER_FOR_TABLE_ATTRIBUTE = "global::EncosyTower.Databases.Authoring.ConverterForTableAttribute";

        public static readonly DiagnosticDescriptor MisplacedConverterAttribute = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0081"
            , title: "Misplaced converter attribute"
            , messageFormat: "The \"{0}\" attribute can only be placed on a type annotated with \"AuthorDatabaseAttribute\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(MisplacedConverterAttribute);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
        }

        private static void Analyze(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol symbol
                || symbol.GetAttribute(AUTHOR_DATABASE_ATTRIBUTE, token) != null
            )
            {
                return;
            }

            foreach (var attrib in symbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attrib.AttributeClass?.HasFullName(CONVERTER_FOR_TABLE_ATTRIBUTE, token) != true)
                {
                    continue;
                }

                var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);

                if (syntax != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          MisplacedConverterAttribute
                        , syntax.GetLocation()
                        , "ConverterForTable"
                    ));
                }
            }
        }
    }
}
