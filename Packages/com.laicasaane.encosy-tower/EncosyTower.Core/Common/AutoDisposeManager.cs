namespace EncosyTower.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using EncosyTower.Collections;

    public static partial class AutoDisposeManager
    {
        private static readonly FasterList<WeakReference<IDisposable>> s_references = new();

        public static void Register([NotNull] IDisposable disposable)
        {
#if !UNITY_EDITOR
            return;

#pragma warning disable CS0162 // Unreachable code detected
#endif

            s_references.Add(new WeakReference<IDisposable>(disposable));

#if !UNITY_EDITOR
#pragma warning restore CS0162 // Unreachable code detected
#endif
        }
    }
}

#if UNITY_EDITOR

namespace EncosyTower.Common
{
    partial class AutoDisposeManager
    {
        internal static void Reset()
        {
            var references = s_references.AsSpan();

            for (int i = references.Length - 1; i >= 0; i--)
            {
                if (references[i].TryGetTarget(out var pool))
                {
                    pool?.Dispose();
                }
                else
                {
                    s_references.RemoveAt(i);
                }
            }
        }
    }
}

namespace EncosyTower.Editor
{
    using UnityEditor;

    [InitializeOnLoad]
    internal static class AutoDisposeManager
    {
        static AutoDisposeManager()
        {
            EditorApplication.playModeStateChanged -= OnEditorStateChange;
            EditorApplication.playModeStateChanged += OnEditorStateChange;
        }

        private static void OnEditorStateChange(PlayModeStateChange stateChange)
        {
            if (EditorSettings.enterPlayModeOptionsEnabled == false
                || EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload) == false
            )
            {
                return;
            }

            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    EncosyTower.Common.AutoDisposeManager.Reset();
                    break;
            }
        }
    }
}

#endif
