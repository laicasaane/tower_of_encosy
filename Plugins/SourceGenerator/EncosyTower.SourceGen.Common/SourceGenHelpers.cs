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
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen
{
    public static class SourceGenHelpers
    {
        public const string TRACKED_NODE_ANNOTATION_USED_BY_ROSLYN = "Id";

        /// <summary>
        /// Line to replace with on generated source.
        /// </summary>
        public const string GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE = "// __generatedline__";

        public const string NEWLINE = "\n";

        public const string OUTPUT_PATH_ADDITIONAL_FILE
            = "encosy-tower-sourcegen-output-path.EncosyTower.SourceGen.Generators.additionalfile";

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

        public struct SourceGenConfig : IEquatable<SourceGenConfig>
        {
            public string projectPath;
            public bool outputSourceGenFiles;

            public readonly override bool Equals(object obj)
                => obj is SourceGenConfig other && Equals(other);

            public readonly bool Equals(SourceGenConfig other)
                => string.Equals(projectPath, other.projectPath, StringComparison.Ordinal)
                && outputSourceGenFiles == other.outputSourceGenFiles
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(projectPath, outputSourceGenFiles);
        }

        public static IncrementalValueProvider<SourceGenConfig> GetSourceGenConfigProvider(
            IncrementalGeneratorInitializationContext context
        )
        {
            var sourceGenConfigProvider = context.AdditionalTextsProvider.Collect().Select((texts, token) => {
                var config = new SourceGenConfig();

                if (texts.Length == 0)
                {
                    return config;
                }

                var index = -1;

                for (var i = 0; i < texts.Length; i++)
                {
                    if (texts[i].Path.EndsWith(OUTPUT_PATH_ADDITIONAL_FILE))
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    return config;
                }

                var path = texts[index].GetText(token)?.ToString();

                if (string.IsNullOrEmpty(path))
                {
                    return config;
                }

                path = path.Replace('\\', '/');

                config.outputSourceGenFiles = Directory.Exists(path);
                config.projectPath = path;

                return config;
            });

            return sourceGenConfigProvider;
        }

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

        public static void OutputSourceToFile(
              SourceProductionContext context
            , Location locationToErrorAt
            , string generatedSourceFilePath
            , SourceText sourceTextForNewClass
            , string projectPath = null
            , string errorCode = "SGE000"
            , string errorTitle = "Generator"
            , string errorCategory = "Generator"
        )
        {
            var resolvedPath = projectPath ?? (CanWriteToProjectPath ? s_projectPath : null);

            if (string.IsNullOrEmpty(resolvedPath))
                return;

            try
            {
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
    }
}
