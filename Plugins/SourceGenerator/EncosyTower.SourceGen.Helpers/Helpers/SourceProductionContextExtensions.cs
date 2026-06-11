using System.IO;
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
            , string projectPath = null
            , Printer? overridePrinter = default
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var outputSource = TypeCreationHelpers.GenerateSourceText(
                  sourceFilePath
                , openingSource
                , bodySource
                , closingSource
                , overridePrinter
            );

            context.AddSource(hintName, outputSource);

            if (outputSourceGenFiles == false || string.IsNullOrEmpty(projectPath))
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var directoryPath = Path.GetDirectoryName(sourceFilePath);
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(sourceFilePath, outputSource.ToString());
        }
    }
}
