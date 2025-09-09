#if UNITY_EDITOR

using System;
using System.Text.RegularExpressions;
using UnityEditor;

namespace EncosyTower.Editor
{
    public static class EncosySerializedPropertyExtensions
    {
        private static readonly Regex s_regexPropertyPath = new(@"^data\[(\d+)\]$", RegexOptions.Compiled);

        public static SerializedProperty FindParentProperty(this SerializedProperty self)
        {
            var paths = self.propertyPath.Split('.');

            if (paths.Length <= 1)
            {
                return default;
            }

            var pathsLength = paths.Length;
            var parentProperty = self.serializedObject.FindProperty(paths[0]);
            var regexPropertyPath = s_regexPropertyPath;

            for (var index = 1; index < pathsLength - 1; index++)
            {
                if (string.Equals(paths[index], "Array", StringComparison.Ordinal))
                {
                    if (index + 1 == pathsLength - 1)
                    {
                        // reached the end
                        break;
                    }

                    if (pathsLength > index + 1 && regexPropertyPath.IsMatch(paths[index + 1]))
                    {
                        var match = regexPropertyPath.Match(paths[index + 1]);
                        var arrayIndex = int.TryParse(match.Groups[1].Value, out var resultIndex) ? resultIndex : 0;
                        parentProperty = parentProperty.GetArrayElementAtIndex(arrayIndex);
                        index++;
                    }
                }
                else
                {
                    parentProperty = parentProperty.FindPropertyRelative(paths[index]);
                }
            }

            return parentProperty;
        }
    }
}

#endif
