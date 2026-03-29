using EncosyTower.Mvvm.ViewBinding.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.Mvvm
{
    [MonoBinder(typeof(GameObject), ExcludeObsolete = true)]
    [MonoBindingProperty(nameof(GameObject.SetActive))]
    public partial class GameObjectBinder { }

    [MonoBinder(typeof(Transform), ExcludeObsolete = true)]
    public partial class TransformBinder { }

    [MonoBinder(typeof(Image), ExcludeObsolete = true)]
    public partial class ImageBinder { }

    [MonoBinder(typeof(Button), ExcludeObsolete = true)]
    public partial class ButtonBinder { }

    [MonoBinder(typeof(ScrollRect), ExcludeObsolete = true)]
    public partial class ScrollRectBinder { }

    [MonoBinder(typeof(TMP_Text), ExcludeObsolete = true)]
    public partial class TMP_TextBinder { }

    public readonly record struct TMP_TextSelectionData(string Text, int StringPosition, int StringSelectPosition);

    [MonoBinder(typeof(TMP_InputField), ExcludeObsolete = true)]
    [MonoBindingCommand(nameof(TMP_InputField.onTextSelection), WrapperType = typeof(TMP_TextSelectionData))]
    public partial class TMP_InputFieldBinder
    {
        private static partial void Unwrap_OnTextSelection(TMP_TextSelectionData value, out string p0, out int p1, out int p2)
        {
            (p0, p1, p2) = value;
        }

        private static partial TMP_TextSelectionData Wrap_OnTextSelection(string p0, int p1, int p2)
        {
            return new(p0, p1, p2);
        }
    }
}
