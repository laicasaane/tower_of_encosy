#if UNITY_TEXTMESHPRO

using System;
using System.Text;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using EncosyTower.Modules.Unions;
using TMPro;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.UnityUI.TextMeshPro
{
    [Serializable]
    [Label("TMP Text", "TextMeshPro")]
    public sealed partial class TMP_TextBinder : MonoBinder<TMP_Text>
    {
    }

    [Serializable]
    [Label("Text", "TMP Text")]
    public sealed partial class TMP_TextBindingText : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetText(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].text = value;
            }
        }
    }

    [Serializable]
    [Label("Text (String Builder)", "TMP Text")]
    public sealed partial class TMP_TextBindingText_StringBuilder : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetText(StringBuilder value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].SetText(value);
            }
        }
    }

    [Serializable]
    [Label("Color", "TMP Text")]
    public sealed partial class TMP_TextBindingColor : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetColor(in Color value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].color = value;
            }
        }
    }

    [Serializable]
    [Label("Color Gradient", "TMP Text")]
    public sealed partial class TMP_TextBindingColorGradient : MonoBindingProperty<TMP_Text>, IBinder
    {
        partial void OnBeforeConstructor()
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (UnionData.BYTE_COUNT >= 64)
            {
                return;
            }
#pragma warning restore CS0162 // Unreachable code detected

            ThrowNotSupported();
        }

        [BindingProperty]
        [field: HideInInspector]
        private void SetColorGradient(VertexGradient value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].colorGradient = value;
            }
        }

        private static void ThrowNotSupported()
        {
            Logging.RuntimeLoggerAPI.LogException(new NotSupportedException(
                "TMP Text Color Gradient binding requires the symbol UNION_64_BYTES or higher to be defined"
            ));
        }
    }

    [Serializable]
    [Label("Max Visible Characters", "TMP Text")]
    public sealed partial class TMP_TextBindingMaxVisibleCharacters : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxVisibleCharacters(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxVisibleCharacters = value;
            }
        }
    }

    [Serializable]
    [Label("Max Visible Words", "TMP Text")]
    public sealed partial class TMP_TextBindingMaxVisibleWords : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxVisibleWords(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxVisibleWords = value;
            }
        }
    }

    [Serializable]
    [Label("Max Visible Lines", "TMP Text")]
    public sealed partial class TMP_TextBindingMaxVisibleLines : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaxVisibleLines(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maxVisibleLines = value;
            }
        }
    }

    [Serializable]
    [Label("Font Asset", "TMP Text")]
    public sealed partial class TMP_TextBindingFontAsset : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFontAsset(TMP_FontAsset value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].font = value;
            }
        }
    }

    [Serializable]
    [Label("Font Size", "TMP Text")]
    public sealed partial class TMP_TextBindingFontSize : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFontSize(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fontSize = value;
            }
        }
    }

    [Serializable]
    [Label("Auto Sizing", "TMP Text")]
    public sealed partial class TMP_TextBindingAutoSizing : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAutoSizing(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].enableAutoSizing = value;
            }
        }
    }

    [Serializable]
    [Label("Font Size Min", "TMP Text")]
    public sealed partial class TMP_TextBindingFontSizeMin : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFontSizeMin(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fontSizeMin = value;
            }
        }
    }

    [Serializable]
    [Label("Font Size Max", "TMP Text")]
    public sealed partial class TMP_TextBindingFontSizeMax : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetFontSizeMax(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].fontSizeMax = value;
            }
        }
    }

    [Serializable]
    [Label("Raycast Target", "TMP Text")]
    public sealed partial class TMP_TextBindingRaycastTarget : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetRaycastTarget(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].raycastTarget = value;
            }
        }
    }

    [Serializable]
    [Label("Maskable", "TMP Text")]
    public sealed partial class TMP_TextBindingMaskable : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMaskable(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].maskable = value;
            }
        }
    }

    [Serializable]
    [Label("Wrapping Mode", "TMP Text")]
    public sealed partial class TMP_TextBindingWrappingMode : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetWrappingMode(TextWrappingModes value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].textWrappingMode = value;
            }
        }
    }

    [Serializable]
    [Label("Overflow Mode", "TMP Text")]
    public sealed partial class TMP_TextBindingOverflowMode : MonoBindingProperty<TMP_Text>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOverflowMode(TextOverflowModes value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].overflowMode = value;
            }
        }
    }
}

#endif
