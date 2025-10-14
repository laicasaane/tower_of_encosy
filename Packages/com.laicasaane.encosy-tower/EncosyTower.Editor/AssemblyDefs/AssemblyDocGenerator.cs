#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EncosyTower.CodeGen;
using EncosyTower.IO;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;

namespace EncosyTower.Editor.AssemblyDefs
{
    internal static class AssemblyDocGenerator
    {
        private readonly record struct AsmdefData(string Name, RootPath Root);

        private record class PackageDef(RootPath Root)
        {
            public readonly List<AsmdefData> Asmdefs = new();
        }

        private const string GENERATED_FILE = ".XMLDOC_CSC_RSP_GENERATED";

        [MenuItem("Encosy Tower/Generate XML Documentation")]
        private static void GenerateXmlDocumentation()
        {
            const string TITLE = "Generate XML Documentation";
            const string INFO = "Generating...";

            EditorUtility.DisplayProgressBar(TITLE, INFO, 0f);

            var guidStrings = AssetDatabase
                .FindAssets($"t:{nameof(AssemblyDefinitionAsset)}")
                .AsSpan();

            var guidStringsLength = guidStrings.Length;

            if (guidStringsLength < 1)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar(TITLE, INFO, 10f);

            RootPath projectRoot = EditorAPI.ProjectPath;

            var packageMap = new Dictionary<string, PackageDef>();

            for (var i = 0; i < guidStringsLength; i++)
            {
                var asmdefGuidString = guidStrings[i];
                var asmdefPath = AssetDatabase.GUIDToAssetPath(asmdefGuidString).Replace('\\', '/');
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(asmdefPath);
                var asmdefAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);

                if (package == null || package.source == PackageSource.Embedded || asmdefAsset.IsInvalid())
                {
                    continue;
                }

                if (packageMap.TryGetValue(package.name, out var packageDef) == false)
                {
                    packageMap[package.name] = packageDef = new(package.resolvedPath);
                }

                try
                {
                    var prefixLength = 9 + package.name.Length + 1;
                    var asmdefFolderPath = asmdefPath[prefixLength..^($"{asmdefAsset.name}.asmdef".Length)];

                    if (asmdefFolderPath.EndsWith('/'))
                    {
                        asmdefFolderPath = asmdefFolderPath[..^1];
                    }

                    RootPath asmdefRoot = packageDef.Root.GetFolderAbsolutePath(asmdefFolderPath);
                    var genenratedFilePath = asmdefRoot.GetFileAbsolutePath(GENERATED_FILE);

                    if (File.Exists(genenratedFilePath) == false)
                    {
                        packageDef.Asmdefs.Add(new AsmdefData(asmdefAsset.name, asmdefRoot));
                    }
                }
                catch (Exception ex)
                {
                    StaticDevLogger.LogError($"Exception occurred while processing '{asmdefPath}': {ex.Message}");
                }
            }

            if (packageMap.Count < 1)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar(TITLE, INFO, 20f);

            var printer = new Printer(1024);

            foreach (var package in packageMap.Values)
            {
                foreach (var asmdef in package.Asmdefs)
                {
                    var cscFilePath = asmdef.Root.GetFileAbsolutePath("csc.rsp");
                    var genenratedFilePath = asmdef.Root.GetFileAbsolutePath(GENERATED_FILE);

                    printer.Clear();

                    var cscFileExists = false;

                    if (File.Exists(cscFilePath))
                    {
                        cscFileExists = true;
                        printer.PrintLine(File.ReadAllText(cscFilePath));
                        File.Delete(cscFilePath);
                    }

                    File.WriteAllText(cscFilePath, GetCscRspContent(ref printer, asmdef.Name), Encoding.UTF8);

                    if (cscFileExists == false)
                    {
                        printer.Clear();

                        File.WriteAllText(
                              asmdef.Root.GetFileAbsolutePath("csc.rsp.meta")
                            , GetCscRspMetaContent(ref printer)
                            , Encoding.UTF8
                        );
                    }

                    File.WriteAllText(genenratedFilePath, "");
                }
            }

            EditorUtility.DisplayProgressBar(TITLE, INFO, 100f);
            EditorUtility.ClearProgressBar();
        }

        private static string GetCscRspContent(ref Printer p, string asmdefName)
        {
            p.PrintBeginLine($"-doc:Library/ScriptAssemblies/{asmdefName}.xml ")
                .Print("-nowarn:1570 -nowarn:1591 -nowarn:1584 -nowarn:1658 -nowarn:419 ")
                .PrintEndLine("-nowarn:1574 -nowarn:1572 -nowarn:1573 -nowarn:1587");
            p.PrintEndLine();

            return p.Result;
        }

        private static string GetCscRspMetaContent(ref Printer p)
        {
            p.PrintLine("fileFormatVersion: 2");
            p.PrintBeginLine("guid: ").PrintEndLine(Guid.NewGuid().ToString("N"));
            p.PrintLine("DefaultImporter:");
            p.PrintLine("  externalObjects: {}");
            p.PrintLine("  userData: ");
            p.PrintLine("  assetBundleName: ");
            p.PrintLine("  assetBundleVariant: ");
            p.PrintEndLine();

            return p.Result;
        }
    }
}

#endif
