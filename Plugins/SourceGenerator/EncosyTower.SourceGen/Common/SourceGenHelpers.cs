// com.unity.entities © 2024 Unity Technologies
//
// Licensed under the Unity Companion License for Unity-dependent projects
// (see https://unity3d.com/legal/licenses/unity_companion_license).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS”
// BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// Please review the license for details on these and other terms and conditions.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class SourceGenHelpers
    {
        public const string TRACKED_NODE_ANNOTATION_USED_BY_ROSLYN = "Id";

        /// <summary>
        /// Line to replace with on generated source.
        /// </summary>
        public const string GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE = "// __generatedline__";

        public const string NEWLINE = "\n";

        private static string s_projectPath = string.Empty;

        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(s_projectPath))
                {
                    throw new Exception(
                        "ProjectPath must set before use, this is also only permitted before 2020."
                    );
                }

                return s_projectPath;
            }
            set => s_projectPath = value;
        }

        public static bool CanWriteToProjectPath => !string.IsNullOrEmpty(s_projectPath);

        public struct SourceGenConfig
        {
            public string projectPath;
            public bool outputSourceGenFiles;
        }

        public struct ParseOptionConfig
        {
            public bool pathIsInFirstAdditionalTextItem;
            public bool outputSourceGenFiles;
        }

        public static IncrementalValueProvider<SourceGenConfig>
            GetSourceGenConfigProvider(IncrementalGeneratorInitializationContext context)
        {
            // Generate provider that lazily provides options based off of context's parse options
            var parseOptionConfigProvider = context.ParseOptionsProvider.Select((options, token) =>
            {
                var parseOptionsConfig = new ParseOptionConfig();

                // Is Unity 2021.1+ and not dots runtime
                var inUnity2021OrNewer = false;

                foreach (var symbolName in options.PreprocessorSymbolNames)
                {
                    inUnity2021OrNewer |= symbolName == "UNITY_2021_1_OR_NEWER";
                    parseOptionsConfig.outputSourceGenFiles |= symbolName == "ENCOSY_OUTPUT_SOURCEGEN_FILES";
                }

                parseOptionsConfig.pathIsInFirstAdditionalTextItem = inUnity2021OrNewer;

                return parseOptionsConfig;
            });

            // Combine the AdditionalTextsProvider with the provider constructed above to provide all SourceGenConfig options lazily
            var sourceGenConfigProvider = context.AdditionalTextsProvider.Collect()
                .Combine(parseOptionConfigProvider)
                .Select((lTextsRIsInsideText, token) =>
                {
                    var config = new SourceGenConfig {
                        outputSourceGenFiles = lTextsRIsInsideText.Right.outputSourceGenFiles
                    };

                    if (Environment.GetEnvironmentVariable("SOURCEGEN_DISABLE_PROJECT_PATH_OUTPUT") == "1")
                        return config;

                    var texts = lTextsRIsInsideText.Left;
                    var projectPathIsInFirstAdditionalTextItem = lTextsRIsInsideText.Right.pathIsInFirstAdditionalTextItem;

                    if (texts.Length == 0 || string.IsNullOrEmpty(texts[0].Path))
                        return config;

                    var path = projectPathIsInFirstAdditionalTextItem ? texts[0].GetText(token)?.ToString() : texts[0].Path;
                    config.projectPath = path?.Replace('\\', '/');
                    return config;
                });

            return sourceGenConfigProvider;
        }

        public static void Setup(GeneratorExecutionContext context)
        {
            // needs to be disabled for e.g. Sonarqube static code analysis (which also uses analyzers)
            if (Environment.GetEnvironmentVariable("SOURCEGEN_DISABLE_PROJECT_PATH_OUTPUT") == "1")
            {
                return;
            }

            var inUnity2021OrNewer = context.ParseOptions.PreprocessorSymbolNames.Contains("UNITY_2021_1_OR_NEWER");

            if (!context.AdditionalFiles.Any() || string.IsNullOrEmpty(context.AdditionalFiles[0].Path))
                return;

            ProjectPath = (inUnity2021OrNewer ? context.AdditionalFiles[0].GetText().ToString() : context.AdditionalFiles[0].Path).Replace('\\', '/');
        }

        private static string GetTempGeneratedPathToFile(string fileNameWithExtension)
        {
            if (!CanWriteToProjectPath)
                return Path.Combine("Temp", "GeneratedCode");

            var tempFileDirectory = Path.Combine(ProjectPath, "Temp", "GeneratedCode");
            Directory.CreateDirectory(tempFileDirectory);
            return Path.Combine(tempFileDirectory, fileNameWithExtension);
        }

        public static SyntaxList<AttributeListSyntax> GetCompilerGeneratedAttribute()
            => AttributeListFromAttributeName("global::System.Runtime.CompilerServices.CompilerGenerated");

        private static SyntaxList<AttributeListSyntax> AttributeListFromAttributeName(string attributeName)
            => new(AttributeList(SingletonSeparatedList(Attribute(IdentifierName(attributeName)))));

        public static void LogInfo(string message)
        {
            if (!CanWriteToProjectPath)
                return;

            // Ignore IO exceptions in case there is already a lock, could use a named mutex but don't want to eat the performance cost
            try
            {
                using StreamWriter w = File.AppendText(GetTempGeneratedPathToFile("SourceGen.log"));
                w.WriteLine(message);
            }
            catch (IOException) { }
        }

        public static class CompilerError
        {
            public static string WithMessage(string errorMessage)
                => "This error indicates a bug in the DOTS source generators. " +
                "We'd appreciate a bug report (Help -> Report a Bug...). Thanks! " +
                $"Error message: '{errorMessage}'";
        }

        public static void LogError(
              this GeneratorExecutionContext context
            , string errorCode
            , string title
            , string errorMessage
            , Location location
            , string description = ""
        )
        {
            if (errorCode.Contains("ICE"))
                errorMessage = CompilerError.WithMessage(errorMessage);

            context.Log(DiagnosticSeverity.Error, errorCode, title, errorMessage, location, description);
        }

        private static void LogInfo(
              this GeneratorExecutionContext context
            , string errorCode
            , string title
            , string errorMessage
            , Location location
            , string description = ""
        )
            => context.Log(DiagnosticSeverity.Info, errorCode, title, errorMessage, location, description);

        private static void Log(
              this GeneratorExecutionContext context
            , DiagnosticSeverity diagnosticSeverity
            , string errorCode
            , string title
            , string errorMessage
            , Location location
            , string description = ""
        )
        {
            LogInfo($"{diagnosticSeverity}: {errorCode}, {title}, {errorMessage}");
            var rule = new DiagnosticDescriptor(errorCode, title, errorMessage, "Source Generator", diagnosticSeverity, true, description);
            context.ReportDiagnostic(Diagnostic.Create(rule, location));
        }

        public static bool TryParseQualifiedEnumValue<TEnum>(string value, out TEnum result)
            where TEnum : struct
        {
            var unqualifiedEnumValue = value.Split('.').Last();
            return Enum.TryParse(unqualifiedEnumValue, out result) && Enum.IsDefined(typeof(TEnum), result);
        }

        public static IEnumerable<Enum> GetFlags(this Enum e)
            => Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);

        public static SourceText WithInitialLineDirectiveToGeneratedSource(
              this SourceText sourceText
            , string generatedSourceFilePath
        )
        {
            var firstLine = sourceText.Lines.FirstOrDefault();
            return sourceText.WithChanges(new TextChange(
                  firstLine.Span
                , $"#line 2 \"{generatedSourceFilePath}\"{NEWLINE}{firstLine}"
            ));
        }

        public static SourceText WithIgnoreUnassignedVariableWarning(this SourceText sourceText)
        {
            var firstLine = sourceText.Lines.FirstOrDefault();
            return sourceText.WithChanges(new TextChange(
                  firstLine.Span
                , $"#pragma warning disable 0219{NEWLINE}{firstLine}"
            ));
        }

        public static SourceText WithGeneratedLineDirectives(this SourceText sourceText, string generatedSourceFilePath)
        {
            // Add line directives for lines with `GeneratedLineTriviaToGeneratedSource` or #line
            var textChanges = new List<TextChange>();
            var lineBuilder = new StringBuilder();
            var buffer = new char[32 * 1024];

            foreach (var line in sourceText.Lines)
            {
                if (line.Text is not { } text || text.Length < 1)
                {
                    continue;
                }

                var lineText = BuildLine(lineBuilder, buffer, text, line.Span);

                if (lineText.Contains(GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE))
                {
                    textChanges.Add(new TextChange(
                          line.Span
                        , lineText.Replace(
                              GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE
                            , $"#line {line.LineNumber + 2} \"{generatedSourceFilePath}\""
                        )
                    ));
                }
                else if (lineText.Contains("#line") && lineText.TrimStart().IndexOf("#line", StringComparison.Ordinal) != 0)
                {
                    var indexOfLineDirective = lineText.IndexOf("#line", StringComparison.Ordinal);
                    textChanges.Add(new TextChange(
                          line.Span
                        , lineText.Substring(0, indexOfLineDirective - 1)
                            + NEWLINE
                            + lineText.Substring(indexOfLineDirective)
                    ));
                }
            }

            return sourceText.WithChanges(textChanges);

            static string BuildLine(StringBuilder builder, char[] buffer, SourceText text, in TextSpan span)
            {
                builder.Clear();

                var textLength = text.Length;

                if (span.End > textLength)
                {
                    return string.Empty;
                }

                int position = Math.Max(Math.Min(span.Start, textLength), 0);
                int length = Math.Min(span.End, textLength) - position;

                builder.EnsureCapacity(length);

                while (position < textLength && length > 0)
                {
                    int copyLength = Math.Min(buffer.Length, length);

                    text.CopyTo(position, buffer, 0, copyLength);
                    builder.Append(buffer, 0, copyLength);

                    length -= copyLength;
                    position += copyLength;
                }

                return builder.ToString();
            }
        }

        // Stable version of String.GetHashCode
        public static int GetStableHashCode(string str)
        {
            unchecked
            {
                var hash1 = 5381;
                var hash2 = hash1;
                var span = str.AsSpan();

                for (var i = 0; i < span.Length && span[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ span[i];

                    if (i == span.Length - 1 || span[i + 1] == '\0')
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ span[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        // Output as generated source file for debugging/inspection
        public static void OutputSourceToFile(
              GeneratorExecutionContext context
            , string generatedSourceFilePath
            , SourceText sourceTextForNewClass
            , string errorCode = "SGE000"
            , string errorTitle = "Generator"
        )
        {
            if (!CanWriteToProjectPath)
                return;

            try
            {
                LogInfo($"Outputting generated source to file {generatedSourceFilePath}...");
                File.WriteAllText(generatedSourceFilePath, sourceTextForNewClass.ToString());
            }
            catch (IOException ioException)
            {
                context.LogInfo(
                      errorCode
                    , errorTitle
                    , ioException.ToUnityPrintableString()
                    , context.Compilation.SyntaxTrees.First().GetRoot().GetLocation()
                );
            }
        }

        // Output as generated source file for debugging/inspection
        public static void OutputSourceToFile(
              SourceProductionContext context
            , Location locationToErrorAt
            , string generatedSourceFilePath
            , SourceText sourceTextForNewClass
            , string errorCode = "SGE000"
            , string errorTitle = "Generator"
            , string errorCategory = "Generator"
        )
        {
            if (!CanWriteToProjectPath)
                return;

            try
            {
                LogInfo($"Outputting generated source to file {generatedSourceFilePath}...");
                File.WriteAllText(generatedSourceFilePath, sourceTextForNewClass.ToString());
            }
            catch (IOException ioException)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                      errorCode
                    , errorTitle
                    , ioException.ToUnityPrintableString()
                    , errorCategory
                    , DiagnosticSeverity.Error
                    , true
                ), locationToErrorAt));
            }
        }

        /// <summary>
        /// Returns true if running as part of csc.exe, otherwise we are likely running in the IDE.
        /// Skipping Source Generation in the IDE can be a considerable performance win as source
        /// generators can be run multiple times per keystroke. If the user doesn't rely on generated types
        /// consider skipping your Generator's Execute method when this returns false
        /// </summary>
        public static readonly bool IsBuildTime = Assembly.GetEntryAssembly() != null;

        public static bool ShouldRun(
              Compilation compilation
            , CancellationToken cancellationToken
            , string assemblyName = ""
            , string referenceAssemblyName = ""
        )
        {
            // Throw if we are cancelled
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(assemblyName) == false
                && compilation.Assembly.Name == assemblyName)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(referenceAssemblyName) == false
                && compilation.ReferencedAssemblyNames.Any(x => x.Name == referenceAssemblyName)
            )
            {
                return true;
            }

            return false;
        }
    }
}
