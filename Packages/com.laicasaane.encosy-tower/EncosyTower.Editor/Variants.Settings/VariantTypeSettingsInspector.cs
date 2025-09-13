using EncosyTower.Editor.Settings;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Variants.Settings
{
    [CustomEditor(typeof(VariantTypeSettings), true)]
    internal sealed class VariantTypeSettingsInspector : UnityEditor.Editor
    {
        private VariantTypeSettings _settings;

        private void OnEnable()
        {
            _settings = target as VariantTypeSettings;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            ViewAPI.ApplyStyleSheetsTo(root, true);

            var button = new Button(OpenSettingsWindow) {
                text = "Open Variant Type Settings Window",
            };

            button.AddToClassList("button-open-settings-window");
            root.Add(button);
            return root;
        }

        private void OpenSettingsWindow()
        {
            if (_settings.IsValid())
            {
                _settings.OpenSettingsWindow();
            }
        }
    }
}
