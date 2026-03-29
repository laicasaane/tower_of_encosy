#if UNITY_EDITOR

using System;

namespace EncosyTower.Editor.Mvvm
{
    internal static class MvvmCodeGenAPI
    {
        public readonly static TypeGroup[] UnityTypes = new TypeGroup[] {
#if UNITY_ANIMATION
            new("UNITY_ANIMATION", new[] {
                typeof(UnityEngine.AnimationClip),
            }),
#endif
#if UNITY_AUDIO
            new("UNITY_AUDIO", new[] {
                typeof(UnityEngine.AudioClip),
                typeof(UnityEngine.Audio.AudioMixer),
            }),
#endif
#if UNITY_6000_0_OR_NEWER
            new("UNITY_6000_0_OR_NEWER", new[] {
                typeof(UnityEngine.TextCore.Text.FontAsset),
            }),
#endif
            new("", new[] {
                typeof(UnityEngine.GameObject),
                typeof(UnityEngine.Material),
                typeof(UnityEngine.Playables.PlayableAsset),
                typeof(UnityEngine.ScriptableObject),
                typeof(UnityEngine.Sprite),
                typeof(UnityEngine.U2D.SpriteAtlas),
                typeof(UnityEngine.Texture),
            }),
#if UNITY_UI_ELEMENTS
            new("UNITY_UI_ELEMENTS", new[] {
                typeof(UnityEngine.UIElements.PanelSettings),
                typeof(UnityEngine.UIElements.StyleSheet),
                typeof(UnityEngine.UIElements.ThemeStyleSheet),
                typeof(UnityEngine.UIElements.VisualTreeAsset),
            }),
#endif
#if UNITY_TEXTMESHPRO
            new("UNITY_TEXTMESHPRO", new[] {
                typeof(TMPro.TMP_FontAsset)
            }),
#endif
        };

        public readonly record struct TypeGroup(string Condition, Type[] Types);
    }
}

#endif
