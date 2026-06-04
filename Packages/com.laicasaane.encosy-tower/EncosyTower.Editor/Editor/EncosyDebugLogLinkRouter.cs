#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace EncosyTower.Editor
{
    [InitializeOnLoad]
    internal static class EncosyDebugLogLinkRouter
    {
        static EncosyDebugLogLinkRouter()
        {
            EditorGUI.hyperLinkClicked -= OnDebugLogLinkClicked;
            EditorGUI.hyperLinkClicked += OnDebugLogLinkClicked;
        }

        private static void OnDebugLogLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            if (args.hyperLinkData.TryGetValue("href", out string href) == false
                || args.hyperLinkData.TryGetValue("router", out string router) == false
                || string.Equals(router, "encosy-tower", StringComparison.OrdinalIgnoreCase) == false
                || href.StartsWith("\\", StringComparison.OrdinalIgnoreCase) == false
            )
            {
                return;
            }

            // Route: Project Settings sub-pages (e.g., "Project/Player")
            if (href.StartsWith("\\open:Project/", StringComparison.Ordinal))
            {
                var path = href[6..];
                SettingsService.OpenProjectSettings(path);
            }
            // Route: User Preferences sub-pages (e.g., "Preferences/External Tools")
            else if (href.StartsWith("\\open:Preferences/", StringComparison.Ordinal))
            {
                var path = href[6..];
                SettingsService.OpenUserPreferences(path);
            }
            // Route: [MenuItem] (e.g., "Encosy Tower/Project Settings/Features")
            else if (href.StartsWith("\\menu:", StringComparison.Ordinal))
            {
                var path = href[6..];
                var success = EditorApplication.ExecuteMenuItem(path);

                if (success == false)
                {
                    LogWarningUndefinedMenuPath(path);
                }
            }
            else
            {
                LogWarningUnsupportedRoute(href);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LogWarningUndefinedMenuPath(string path)
        {
            Debug.LogWarning($"Could not find menu item at path '{path}'" );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LogWarningUnsupportedRoute(string href)
        {
            Debug.LogWarning(
                $"Could not route to '{href}'. " +
                $"Current supported routes are <b>\\open:Project/</b>, <b>\\open:Preferences/</b>, " +
                $" <b>\\menu:</b>."
            );
        }
    }
}

#endif
