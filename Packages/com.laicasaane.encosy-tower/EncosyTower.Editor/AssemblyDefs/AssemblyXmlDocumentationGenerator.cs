#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EncosyTower.CodeGen;
using EncosyTower.IO;
using EncosyTower.Logging;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;

namespace EncosyTower.Editor.AssemblyDefs
{
    internal static class AssemblyXmlDocumentationGenerator
    {
        private readonly struct AsmdefXmlDoc
        {
            public readonly string Name;
            public readonly RootPath Root;

            public AsmdefXmlDoc(string name, RootPath root)
            {
                Name = name;
                Root = root;
            }
        }

        private class PackageDef
        {
            public readonly RootPath Root;
            public readonly List<AsmdefXmlDoc> Asmdefs = new();

            public PackageDef(RootPath root)
            {
                Root = root;
            }
        }

        private const string GENERATED_FILE = ".XMLDOC_CSC_RSP_GENERATED";
        private const string XML_DOCUMENTATION_FOLDER = "Library/XmlDocumentationGenerated";
        private const string SCRIPT_ASSEMBLIES_FOLDER = "Library/ScriptAssemblies";

        [MenuItem("Encosy Tower/XML Documentation/Generate", priority = 88_00_00_00)]
        private static void GenerateXmlDocumentation()
        {
            const string TITLE = "Generate XML Documentation";
            const string INFO = "Generating...";

            EditorUtility.DisplayProgressBar(TITLE, INFO, 0f);

            var packages = GetPackageDefs(false);

            if (packages.Length < 1)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar(TITLE, INFO, 20f);

            var projectRoot = GetProjectRootPath();
            var xmlDocumentationFolderPath = projectRoot.GetFolderAbsolutePath(XML_DOCUMENTATION_FOLDER);

            if (Directory.Exists(xmlDocumentationFolderPath) == false)
            {
                Directory.CreateDirectory(xmlDocumentationFolderPath);
            }

            var printer = new Printer(1024);

            foreach (var package in packages)
            {
                foreach (var asmdef in package.Asmdefs)
                {
                    var cscFilePath = asmdef.Root.GetFileAbsolutePath("csc.rsp");
                    var cscBackupFilePath = asmdef.Root.GetFileAbsolutePath(".csc-rsp-backup");
                    var generatedFilePath = asmdef.Root.GetFileAbsolutePath(GENERATED_FILE);
                    var xmlFilePath = $"{XML_DOCUMENTATION_FOLDER}/{asmdef.Name}.xml";

                    printer.Clear();

                    var cscFileExists = false;

                    if (File.Exists(cscFilePath))
                    {
                        cscFileExists = true;
                        printer.PrintLine(File.ReadAllText(cscFilePath));
                        File.Copy(cscFilePath, cscBackupFilePath);
                        File.Delete(cscFilePath);
                    }

                    File.WriteAllText(cscFilePath, GetCscRspContent(ref printer, xmlFilePath), Encoding.UTF8);

                    if (cscFileExists == false)
                    {
                        printer.Clear();

                        File.WriteAllText(
                              asmdef.Root.GetFileAbsolutePath("csc.rsp.meta")
                            , GetCscRspMetaContent(ref printer)
                            , Encoding.UTF8
                        );
                    }

                    File.WriteAllText(generatedFilePath, "");
                }
            }

            EditorUtility.DisplayProgressBar(TITLE, INFO, 100f);
            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        [MenuItem("Encosy Tower/XML Documentation/Delete", priority = 88_00_00_01)]
        private static void DeleteXmlDocumentation()
        {
            const string TITLE = "Delete XML Documentation";
            const string INFO = "Deleting...";

            EditorUtility.DisplayProgressBar(TITLE, INFO, 0f);

            var packages = GetPackageDefs(true);

            if (packages.Length < 1)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            foreach (var package in packages)
            {
                foreach (var asmdef in package.Asmdefs)
                {
                    var cscFilePath = asmdef.Root.GetFileAbsolutePath("csc.rsp");
                    var cscBackupFilePath = asmdef.Root.GetFileAbsolutePath(".csc-rsp-backup");
                    var cscMetaFilePath = asmdef.Root.GetFileAbsolutePath("csc.rsp.meta");
                    var generatedFilePath = asmdef.Root.GetFileAbsolutePath(GENERATED_FILE);

                    if (File.Exists(cscBackupFilePath))
                    {
                        File.Copy(cscBackupFilePath, cscFilePath, true);
                        File.Delete(cscBackupFilePath);
                    }
                    else if (File.Exists(cscFilePath))
                    {
                        File.Delete(cscFilePath);
                        File.Delete(cscMetaFilePath);
                    }

                    if (File.Exists(generatedFilePath))
                    {
                        File.Delete(generatedFilePath);
                    }
                }
            }

            var projectRoot = GetProjectRootPath();
            var xmlDocumentationFolderPath = projectRoot.GetFolderAbsolutePath(XML_DOCUMENTATION_FOLDER);

            if (Directory.Exists(xmlDocumentationFolderPath))
            {
                Directory.Delete(xmlDocumentationFolderPath, true);
            }

            EditorUtility.DisplayProgressBar(TITLE, INFO, 100f);
            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        private static PackageDef[] GetPackageDefs(bool onlyGenerated)
        {
            var guidStrings = AssetDatabase
                .FindAssets($"t:{nameof(AssemblyDefinitionAsset)}")
                .AsSpan();

            var guidStringsLength = guidStrings.Length;

            if (guidStringsLength < 1)
            {
                EditorUtility.ClearProgressBar();
                return Array.Empty<PackageDef>();
            }

            var projectRoot = GetProjectRootPath();

            var packageMap = new Dictionary<string, PackageDef>();

            for (var i = 0; i < guidStringsLength; i++)
            {
                var asmdefGuidString = guidStrings[i];
                var asmdefPath = AssetDatabase.GUIDToAssetPath(asmdefGuidString).Replace('\\', '/');
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(asmdefPath);
                var asmdefAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);

                if (package == null || package.source == PackageSource.Embedded || asmdefAsset == false)
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

                    var asmdefRoot = packageDef.Root.GetFolderRoot(asmdefFolderPath);
                    var genenratedFilePath = asmdefRoot.GetFileAbsolutePath(GENERATED_FILE);
                    var generatedFileExists = File.Exists(genenratedFilePath);

                    if ((onlyGenerated && generatedFileExists)
                        || (onlyGenerated == false && generatedFileExists == false)
                    )
                    {
                        packageDef.Asmdefs.Add(new AsmdefXmlDoc(asmdefAsset.name, asmdefRoot));
                    }
                }
                catch (Exception ex)
                {
                    StaticDevLogger.LogError($"Exception occurred while processing '{asmdefPath}': {ex.Message}");
                }
            }

            return packageMap.Values.ToArray();
        }

        [InitializeOnLoadMethod]
        private static void CopyXmlDocToScriptAssembliesFolder()
        {
            var projectRoot = GetProjectRootPath();
            var xmlDocumentationFolderPath = projectRoot.GetFolderAbsolutePath(XML_DOCUMENTATION_FOLDER);

            if (Directory.Exists(xmlDocumentationFolderPath) == false)
            {
                return;
            }

            var xmlFiles = Directory.GetFiles(xmlDocumentationFolderPath, "*.xml", SearchOption.TopDirectoryOnly);

            if (xmlFiles.Length < 1)
            {
                return;
            }

            var scriptAssembliesFolderPath = projectRoot.GetFolderAbsolutePath(SCRIPT_ASSEMBLIES_FOLDER);

            foreach (var xmlFile in xmlFiles)
            {
                var fileName = Path.GetFileName(xmlFile);
                var destFilePath = Path.Combine(scriptAssembliesFolderPath, fileName);

                try
                {
                    File.Copy(xmlFile, destFilePath, true);
                }
                catch (Exception ex)
                {
                    StaticDevLogger.LogError($"Failed to copy '{xmlFile}' to '{destFilePath}': {ex.Message}");
                }
            }
        }

        private static RootPath GetProjectRootPath()
            => EditorAPI.ProjectPath;

        private static string GetCscRspContent(ref Printer p, string xmlFilePath)
        {
            // https://gamedev.stackexchange.com/a/173674
            p.PrintLine($"-doc:./{xmlFilePath} ")
             .PrintLine("-nowarn:419")
             .PrintLine("-nowarn:1570")
             .PrintLine("-nowarn:1572")
             .PrintLine("-nowarn:1573")
             .PrintLine("-nowarn:1574")
             .PrintLine("-nowarn:1584")
             .PrintLine("-nowarn:1587")
             .PrintLine("-nowarn:1591")
             .PrintLine("-nowarn:1658")
             .PrintEndLine();

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
