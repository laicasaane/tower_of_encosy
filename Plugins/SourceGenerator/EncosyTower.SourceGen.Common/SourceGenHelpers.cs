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

        public struct SourceGenConfig : IEquatable<SourceGenConfig>
        {
            public const string OUTPUT_PATH_ADDITIONAL_FILE
                = "sourcegen-output-path.EncosyTower.SourceGen.Generators.additionalfile";

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
                    if (texts[i].Path.EndsWith(SourceGenConfig.OUTPUT_PATH_ADDITIONAL_FILE))
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

        public static string BuildSourceFilePath(string assemblyName, string hintName, string projectPath)
        {
            var tempFilePath = GetTempFilePath(assemblyName, hintName);

            return string.IsNullOrEmpty(projectPath)
                ? tempFilePath
                : $"{projectPath}/{tempFilePath}";
        }

        public static string BuildHintName(string assemblyName, string fileName, string toStableHash, int salting)
        {
            var stableHashCode = GetStableHashCode(toStableHash) & 0x7fffffff;
            return BuildHintName(assemblyName, fileName, stableHashCode, salting);
        }

        public static string BuildHintName(string assemblyName, string fileName, int stableHashCode, int salting)
        {
            return $"{fileName}_{assemblyName}_{stableHashCode}_{salting}.g.cs";
        }

        public static string GetTempFilePath(string assemblyName, string hintName)
        {
            return $"Temp/GeneratedCode/{assemblyName}/{hintName}";
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
    }
}
