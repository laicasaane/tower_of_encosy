using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class SourceProductionContextExtensions
    {
        public static void OutputSource(
              this ref SourceProductionContext context
            , bool outputSourceGenFiles
            , SyntaxNode syntax
            , string source
            , string hintName
            , string sourceFilePath
            , Printer? overridePrinter = default
        )
        {
            var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                  sourceFilePath
                , syntax
                , source
                , context.CancellationToken
                , overridePrinter
            );

            context.AddSource(hintName, outputSource);

            if (outputSourceGenFiles)
            {
                SourceGenHelpers.OutputSourceToFile(
                      context
                    , syntax.GetLocation()
                    , sourceFilePath
                    , outputSource
                );
            }
        }

        public static void OutputSource(
              this ref SourceProductionContext context
            , bool outputSourceGenFiles
            , string openingSource
            , string bodySource
            , string closingSource
            , string hintName
            , string sourceFilePath
            , Location location
            , Printer? overridePrinter = default
        )
        {
            var outputSource = TypeCreationHelpers.GenerateSourceText(
                  sourceFilePath
                , openingSource
                , bodySource
                , closingSource
                , overridePrinter
            );

            context.AddSource(hintName, outputSource);

            if (outputSourceGenFiles)
            {
                SourceGenHelpers.OutputSourceToFile(
                      context
                    , location
                    , sourceFilePath
                    , outputSource
                );
            }
        }

        public static void OutputSource(
              this ref SourceProductionContext context
            , bool outputSourceGenFiles
            , SyntaxNode containingSyntax
            , SyntaxNode originalSyntax
            , string source
            , string hintName
            , string sourceFilePath
            , Printer? overridePrinter = default
        )
        {
            var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                  sourceFilePath
                , containingSyntax
                , source
                , context.CancellationToken
                , overridePrinter
            );

            context.AddSource(hintName, outputSource);

            if (outputSourceGenFiles)
            {
                SourceGenHelpers.OutputSourceToFile(
                      context
                    , originalSyntax.GetLocation()
                    , sourceFilePath
                    , outputSource
                );
            }
        }
    }
}
