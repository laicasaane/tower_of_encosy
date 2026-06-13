using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace EncosyTower.DevTools;

internal static class DevToolsForToolbar
{
    private static readonly ToolDefintion[] s_elements = new ToolDefintion[]
    {
          new("Open in IDE", "Open C# project in default IDE", "Assets/Open C# Project", "cs script icon")
        , new("Open in VSCode", "Open C# project in VSCode", "Dev Tools/Open in VSCode", "cs script icon")
        , new("Update Version", "Open version updater", "Dev Tools/Update Version", "creationtoolsgroup")
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
