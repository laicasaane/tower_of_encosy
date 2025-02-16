#if UNITY_EDITOR

// MIT License
//
// Copyright (c) 2024 Mika Notarnicola
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// https://github.com/thebeardphantom/Runtime-TypeCache

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using EncosyTower.CodeGen;
using EncosyTower.Logging;
using EncosyTower.Types.Internals;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditorInternal;
using UnityEngine;

namespace EncosyTower.Types.Editor
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Responsible for creating the <see cref="SerializedTypeCacheAsset"/> at build time.
    /// </summary>
    internal sealed class SerializedTypeCacheBuilder : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// The name of which the debug copies of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_NAME = nameof(SerializedTypeCacheAsset);

        /// <summary>
        /// The root directory at which the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_ROOT_DIRECTORY = $"Assets/{nameof(RuntimeTypeCache)}";

        /// <summary>
        /// The directory at which the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_DIRECTORY = $"{ASSET_ROOT_DIRECTORY}/{nameof(Resources)}";

        /// <summary>
        /// The path at which the transitory <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_PATH = $"{ASSET_DIRECTORY}/{ASSET_FILE_NAME}.asset";

        /// <summary>
        /// The name of which the debug copies of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_NAME_DEBUG = $"{ASSET_FILE_NAME}_Debug";

        /// <summary>
        /// The path at which the debug copy of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_FILE_PATH_DEBUG = $"Temp/{ASSET_FILE_NAME_DEBUG}.asset";

        /// <summary>
        /// The path at which the debug json copy of the <see cref="SerializedTypeCacheAsset"/> will be created.
        /// </summary>
        private const string ASSET_JSON_FILE_PATH_DEBUG = $"Temp/{ASSET_FILE_NAME_DEBUG}.json";

        /// <summary>
        /// The path at which the 'link.xml' file will be created.
        /// </summary>
        private const string LINK_XML_FILE_PATH = $"{ASSET_ROOT_DIRECTORY}/link.xml";

        /// <summary>
        /// The path at which the debug copy of 'link.xml' file will be created.
        /// </summary>
        private const string LINK_XML_FILE_PATH_DEBUG = $"Temp/{ASSET_FILE_NAME_DEBUG}_link.xml";

        [MenuItem("Encosy Tower/Runtime Type Cache/Create Debug Assets")]
        private static void Menu_CreateDebugAssets()
        {
            CreateAssets(onlyDebug: true);

            DevLoggerAPI.LogInfo($"<a href=\"{ASSET_FILE_PATH_DEBUG}\">{ASSET_FILE_NAME_DEBUG}.asset</a>");
            DevLoggerAPI.LogInfo($"<a href=\"{ASSET_JSON_FILE_PATH_DEBUG}\">{ASSET_FILE_NAME_DEBUG}.json</a>");
            DevLoggerAPI.LogInfo($"<a href=\"{LINK_XML_FILE_PATH_DEBUG}\">{ASSET_FILE_NAME_DEBUG}_link.xml</a>");
        }

        [MenuItem("Encosy Tower/Runtime Type Cache/Create Runtime Assets")]
        private static void Menu_CreateRuntimeAssets()
        {
            CreateAssets(onlyDebug: false);

            DevLoggerAPI.LogInfo($"<a href=\"{ASSET_FILE_PATH}\">{ASSET_FILE_NAME}.asset</a>");
            DevLoggerAPI.LogInfo($"<a href=\"{LINK_XML_FILE_PATH}\">link.xml</a>");
        }

        private static void CreateAssets(bool onlyDebug)
        {
            try
            {
                // Generate type cache asset
                var asset = ScriptableObject.CreateInstance<SerializedTypeCacheAsset>();
                asset._cache.Regenerate(out var linkXmlTypeStore);

                // Serialize asset to temp folder for debug inspection
                asset.name = ASSET_FILE_NAME_DEBUG;

                InternalEditorUtility.SaveToSerializedFileAndForget(
                      new Object[] { asset }
                    , ASSET_FILE_PATH_DEBUG
                    , true
                );

                if (Directory.Exists("Temp"))
                {
                    // Serialize to json for debug inspection
                    var json = JsonUtility.ToJson(asset, true);
                    json = Regex.Replace(json, "(<|>|k__BackingField)", "");

                    File.WriteAllText(ASSET_JSON_FILE_PATH_DEBUG, json);
                }

                if (onlyDebug == false && Directory.Exists(ASSET_DIRECTORY) == false)
                {
                    Directory.CreateDirectory(ASSET_DIRECTORY);
                }

                if (onlyDebug == false)
                {
                    // Create asset so preloaded assets can actually use it
                    asset.name = ASSET_FILE_NAME;
                    AssetDatabase.CreateAsset(asset, ASSET_FILE_PATH);

                    var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();

                    preloadedAssets.Add(asset);
                    preloadedAssets.RemoveAll(static obj => obj == false);

                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                CreateLinkXmlFile(linkXmlTypeStore, onlyDebug);
            }
            catch (Exception)
            {
                DeleteAndRemoveAsset();
                throw;
            }
        }

        /// <summary>
        /// Cleans up after the <see cref="IPreprocessBuildWithReport.OnPreprocessBuild" /> function.
        /// </summary>
        private static void DeleteAndRemoveAsset()
        {
            AssetDatabase.DeleteAsset(ASSET_DIRECTORY);
            AssetDatabase.DeleteAsset(LINK_XML_FILE_PATH);

            var files = Directory.GetFiles($"{Application.dataPath}/../{ASSET_ROOT_DIRECTORY}");

            if (files.Length < 1)
            {
                AssetDatabase.DeleteAsset(ASSET_ROOT_DIRECTORY);
            }

            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(static obj => obj == false || obj is SerializedTypeCacheAsset);

            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateLinkXmlFile(LinkXmlTypeStore linkXmlTypeStore, bool onlyDebug)
        {
            var unityObjectType = typeof(UnityEngine.Object);
            var p = Printer.DefaultLarge;
            var store = linkXmlTypeStore.Store;
            var assemblies = store.Keys.ToList();
            assemblies.Sort(static (x, y) => string.CompareOrdinal(x.FullName, y.FullName));

            p.OpenScope("<linker>");
            {
                foreach (var assembly in assemblies)
                {
                    var typeToMembersMap = store[assembly];

                    if (typeToMembersMap.Count < 1)
                    {
                        continue;
                    }

                    p.PrintBeginLine("<assembly fullname=\"").Print(assembly.FullName).PrintEndLine("\">");
                    p = p.IncreasedIndent();
                    {
                        var indexedTypes = typeToMembersMap.Keys.ToList();
                        indexedTypes.Sort(static (x, y) => x.index.CompareTo(y.index));

                        foreach (var indexedType in indexedTypes)
                        {
                            var type = indexedType.Type;
                            var members = typeToMembersMap[indexedType];

                            p.PrintBeginLine("<type fullname=\"").Print(type.FullName).Print("\"");

                            if (unityObjectType.IsAssignableFrom(type) == false
                                && type.GetCustomAttribute<System.SerializableAttribute>() != null
                            )
                            {
                                p.Print("serialized=\"true\"");
                            }

                            if (members.Count < 1)
                            {
                                p.PrintEndLine(" />");
                                continue;
                            }

                            p.PrintEndLine(">");
                            p = p.IncreasedIndent();
                            {
                                var sortedMembers = members.OrderBy(static x => x.MemberType).ToList();

                                foreach (var member in sortedMembers)
                                {
                                    if (member is FieldInfo field)
                                    {
                                        p.PrintBeginLine("<field signature=\"")
                                            .Print(field.FieldType.FullName)
                                            .Print(" ")
                                            .Print(field.Name)
                                            .PrintEndLine("\" />");
                                    }
                                    else if (member is MethodInfo method)
                                    {
                                        p.PrintBeginLine("<method signature=\"")
                                            .Print(method.ReturnType.FullName)
                                            .Print(" ")
                                            .Print(method.Name)
                                            .Print("(");

                                        var args = method.GetParameters().AsSpan();
                                        var argsLength = args.Length;

                                        for (var k = 0; k < argsLength; k++)
                                        {
                                            p.Print(args[k].ParameterType.FullName);

                                            if (k < argsLength - 1)
                                            {
                                                p.Print(",");
                                            }
                                        }

                                        p.Print(")").PrintEndLine("\" />");
                                    }
                                }
                            }
                            p = p.DecreasedIndent();
                            p.PrintLine("</type>");
                        }
                    }
                    p = p.DecreasedIndent();
                    p.PrintLine("</assembly>");
                }
            }
            p.CloseScope("</linker>");
            p.PrintEndLine();

            if (onlyDebug)
            {
                File.WriteAllText(LINK_XML_FILE_PATH_DEBUG, p.Result);
            }
            else
            {
                File.WriteAllText(LINK_XML_FILE_PATH, p.Result);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <inheritdoc />
        int IOrderedCallback.callbackOrder { get; }

        /// <inheritdoc />
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            CreateAssets(onlyDebug: false);
        }

        /// <inheritdoc />
        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            DeleteAndRemoveAsset();
        }
    }
}

#endif
