#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Editor.ProjectSetup
{
    internal sealed class MissingScriptingSymbolsException : Exception
    {
        public MissingScriptingSymbolsException(string message) : base(message)
        {
        }
    }

    internal class RequireScriptingSymbolsBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            EditorBuildSettingsScene[] buildScenes;
            HashSet<string> buildSymbols;

            if (BuildProfile.GetActiveBuildProfile() is BuildProfile buildProfile)
            {
                buildScenes = buildProfile.GetScenesForBuild();
                buildSymbols = buildProfile.scriptingDefines.ToHashSet();
            }
            else
            {
                buildScenes = EditorBuildSettings.scenes;
                buildSymbols = BuildAPI.GetScriptingDefineSymbols(BuildAPI.ActiveNamedBuildTarget);
            }

            var result = new List<SceneMissingSymbols>();
            var gameObjects = new List<GameObject>();

            foreach (var buildScene in buildScenes)
            {
                GetMissingSymbols(buildScene, gameObjects, buildSymbols, out var sceneMissingSymbols);

                if (sceneMissingSymbols is { Count: > 0 })
                {
                    result.Add(sceneMissingSymbols);
                }
            }

            if (result.Count < 1)
            {
                return;
            }

            var sb = new StringBuilder(1024)
                .AppendLine("Some scripting symbols are required to build the scenes.");

            foreach (var (name, symbols) in result)
            {
                sb.Append($"- Scene '").Append(name).Append("' requires: ");

                for (var i = 0; i < symbols.Count; i++)
                {
                    sb.Append(symbols[i]);

                    if (i < symbols.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append('\n');
            }

            throw new MissingScriptingSymbolsException(sb.ToString());
        }

        private static void GetMissingSymbols(
              EditorBuildSettingsScene buildScene
            , List<GameObject> gameObjects
            , HashSet<string> buildSymbols
            , out SceneMissingSymbols result
        )
        {
            gameObjects.Clear();

            var scene = SceneManager.GetSceneByPath(buildScene.path);

            if (scene.IsValid() == false)
            {
                result = default;
                return;
            }

            try
            {
                scene.GetRootGameObjects(gameObjects);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                result = default;
                return;
            }

            result = new SceneMissingSymbols(scene.name, buildSymbols.Count);

            Debug.Log($"Validating missing scripting symbols on scene '{scene.name}'...");

            foreach (var gameObject in gameObjects)
            {
                if (gameObject.CompareTag("EditorOnly") == false)
                {
                    continue;
                }

                if (gameObject.TryGetComponent<RequiredScriptingSymbols>(out var component) == false)
                {
                    continue;
                }

                foreach (var symbol in component._symbols.AsSpan())
                {
                    if (buildSymbols.Contains(symbol) == false)
                    {
                        result.Symbols.Add(symbol);
                    }
                }
            }
        }

        private class SceneMissingSymbols
        {
            public readonly string Name;
            public readonly List<string> Symbols;

            public SceneMissingSymbols(string name, int capacity)
            {
                Name = name;
                Symbols = new(capacity);
            }

            public int Count => Symbols.Count;

            public void Deconstruct(out string name, out List<string> symbols)
            {
                name = Name;
                symbols = Symbols;
            }
        }
    }
}

#endif
