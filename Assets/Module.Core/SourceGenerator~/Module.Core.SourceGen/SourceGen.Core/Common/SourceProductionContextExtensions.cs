using Microsoft.CodeAnalysis;

namespace Module.Core.SourceGen
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
        )
        {
            var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                  sourceFilePath
                , syntax
                , source
                , context.CancellationToken
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
    }
}
