#if UNITY_EDITOR

using System;

namespace EncosyTower.Editor.Mvvm
{
    internal static class MvvmCodeGenAPI
    {
        public readonly static Type[] UnityTypes = new Type[] {
            typeof(UnityEngine.AnimationClip),
            typeof(UnityEngine.AudioClip),
            typeof(UnityEngine.Audio.AudioMixer),
#if UNITY_6000_0_OR_NEWER
            typeof(UnityEngine.TextCore.Text.FontAsset),
#endif
            typeof(UnityEngine.GameObject),
            typeof(UnityEngine.Material),
            typeof(UnityEngine.UIElements.PanelSettings),
            typeof(UnityEngine.Playables.PlayableAsset),
            typeof(UnityEngine.ScriptableObject),
            typeof(UnityEngine.Sprite),
            typeof(UnityEngine.U2D.SpriteAtlas),
            typeof(UnityEngine.UIElements.StyleSheet),
            typeof(UnityEngine.Texture),
            typeof(UnityEngine.UIElements.ThemeStyleSheet),
#if UNITY_TEXTMESHPRO
            typeof(TMPro.TMP_FontAsset),
#endif
            typeof(UnityEngine.UIElements.VisualTreeAsset),
        };
    }
}

#endif
