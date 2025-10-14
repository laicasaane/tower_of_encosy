#if UNITY_EDITOR

using System;
using System.IO;
using EncosyTower.Collections;
using EncosyTower.Scenes;
using UnityEditor;

namespace EncosyTower.Editor.Scenes
{
    public static class SceneBuildIndexEditorAPI
    {
        /// <summary>
        /// Check if a <paramref name="index"/> is valid in the current <see cref="EditorBuildSettings"/>.
        /// </summary>
        /// <returns>
        /// True if the <paramref name="index"/> is valid in the current <see cref="EditorBuildSettings"/>;
        /// otherwise, false.
        /// </returns>
        /// <remarks>
        /// A <see cref="SceneBuildIndex"/> is considered valid if its index and name match an entry in the
        /// current <see cref="EditorBuildSettings.scenes"/>.
        /// </remarks>
        public static bool Validate(SceneBuildIndex index)
        {
            var indices = GetSceneBuidIndices();
            return indices.Contains(index);
        }

        private static ListFast<SceneBuildIndex> GetSceneBuidIndices()
        {
            var scenes = EditorBuildSettings.scenes.AsSpan();
            var length = scenes.Length;
            var result = new ListFast<SceneBuildIndex>(new(length));

            for (var i = 0; i < length; i++)
            {
                var scene = scenes[i];
                var name = Path.GetFileName(scene.path);
                result.Add(new(i, name));
            }

            return result;
        }
    }
}

#endif
