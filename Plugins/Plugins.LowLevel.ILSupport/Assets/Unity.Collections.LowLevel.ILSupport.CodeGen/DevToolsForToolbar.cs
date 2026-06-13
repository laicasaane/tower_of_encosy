using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Unity.Collections.LowLevel.ILSupport.CodeGen;

internal static class DevToolsForToolbar
{
    private static readonly ToolDefintion[] s_elements = new ToolDefintion[]
    {
          new("Open in IDE", "Open C# project in default IDE", "Assets/Open C# Project", "cs script icon")
        , new("Open in VSCode", "Open C# project in VSCode", "Dev Tools/Open in VSCode", "cs script icon")
        , new("Copy Assembly", "Copy the generated assembly to the clipboard", "Dev Tools/Copy Assembly", "copy", false)
    };

    [MainToolbarElement("Dev Tools/Tools", defaultDockPosition = MainToolbarDockPosition.Left)]
    public static IEnumerable<MainToolbarElement> CreateAnalysisWindowsBar()
    {
        foreach (var (name, tooltip, menuPath, icon, themed) in s_elements)
        {
            var image = GetIcon(icon, themed);
            var content = new MainToolbarContent(name, image, tooltip);

            yield return new MainToolbarButton(content, OnClick);

            void OnClick()
            {
                EditorApplication.ExecuteMenuItem(menuPath);
            }
        }

        static Texture2D GetIcon(string icon, bool themed)
            => (Texture2D)EditorGUIUtility.IconContent(themed ? Themed(icon) : icon).image;

        static string Themed(string icon)
            => EditorGUIUtility.isProSkin ? $"d_{icon}" : icon;
    }

    [MenuItem("Dev Tools/Open in VSCode")]
    private static void OpenInVSCode()
    {
        var startInfo = new ProcessStartInfo {
            FileName = "code",
            Arguments = ".",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        Process.Start(startInfo);
    }

    [MenuItem("Dev Tools/Copy Assembly")]
    public static void CopyAssembly()
    {
        var assemblyPath = GetAssemblyPath();
        var encosyPackagePluginsPath = GetEncosyPackagePluginsPath();
        var bclRuntimeUnsafePath = GetBclRuntimeUnsafePath();
        var packageDest = Path.Combine(encosyPackagePluginsPath, $"{Constants.ASSEMBLY_NAME}.dll");
        var bclDest = Path.Combine(bclRuntimeUnsafePath, $"{Constants.ASSEMBLY_NAME}.dll");

        File.Copy(assemblyPath, packageDest, true);
        File.Copy(assemblyPath, bclDest, true);
    }

    [MenuItem("Dev Tools/Reveal Assembly")]
    public static void RevealAssembly()
    {
        var path = GetAssemblyPath();

        if (File.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }

    [MenuItem("Dev Tools/Reveal Encosy Package Plugins")]
    public static void RevealEncosyPackagePlugins()
    {
        var path = GetEncosyPackagePluginsPath();

        if (Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }

    [MenuItem("Dev Tools/Reveal Bcl.RuntimeUnsafe")]
    public static void RevealBclRuntimeUnsafe()
    {
        var path = GetBclRuntimeUnsafePath();

        if (Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
    }

    private static string GetAssemblyPath()
    {
        return Path.Combine(
              Application.dataPath
            , $"../Library/ScriptAssemblies/{Constants.ASSEMBLY_NAME}.dll"
        );
    }

    private static string GetEncosyPackagePluginsPath()
    {
        return Path.Combine(
              Application.dataPath
            , $"../../../Packages/com.laicasaane.encosy-tower/Plugins/{Constants.ASSEMBLY_NAME}"
        );
    }

    private static string GetBclRuntimeUnsafePath()
    {
        return Path.Combine(
              Application.dataPath
            , $"../../../Plugins/Bcl.Extensions/Bcl.RuntimeUnsafe"
        );
    }

    private struct ToolDefintion
    {
        public string name;
        public string tooltip;
        public string menuPath;
        public string icon;
        public bool themed;

        public ToolDefintion(
              string name
            , string tooltip
            , string menuPath
            , string icon
            , bool themed = true
        )
        {
            this.name = name;
            this.tooltip = tooltip;
            this.menuPath = menuPath;
            this.icon = icon;
            this.themed = themed;
        }

        public readonly void Deconstruct(
              out string name
            , out string tooltip
            , out string menuPath
            , out string icon
            , out bool themed
        )
        {
            name = this.name;
            tooltip = this.tooltip;
            menuPath = this.menuPath;
            icon = this.icon;
            themed = this.themed;
        }
    }
}
