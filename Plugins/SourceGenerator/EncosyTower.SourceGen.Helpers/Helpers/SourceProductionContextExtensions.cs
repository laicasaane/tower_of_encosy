using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class SourceProductionContextExtensions
    {
        public static void OutputSource(
              this ref SourceProductionContext context
            , bool outputSourceGenFiles
            , string openingSource
            , string bodySource
            , string closingSource
            , string hintName
            , string sourceFilePath
            , Location location
            , string projectPath = null
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
                    , projectPath
                );
            }
        }
    }
}
