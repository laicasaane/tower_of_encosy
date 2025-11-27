using EncosyTower.Variants;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Variants.Settings
{
    internal sealed class VariantTypeSettingsEditor
    {
        private const float SAVE_DELAY = 0.5f;
        private const string UNIT_LABEL = "(VARIANT_{0}_LONGS = {1} ints = {2} bytes)";

        private readonly VariantTypeSettings _settings;
        private readonly SerializedObject _serializedSettings;
        private readonly Label _unitLabel;

        // An optimization to limit the chance the
        // frequent of OtherValueUpdated being invoked.
        private bool _otherValueUpdated;
        private double _markTime;

        public VariantTypeSettingsEditor(
              VariantTypeSettings settings
            , SerializedObject serializedSettings
            , VisualElement root
        )
        {
            _settings = settings;
            _serializedSettings = serializedSettings;

            ViewAPI.ApplyStyleSheetsTo(root, true);

            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = "Variant Type",
            };

            titleBar.AddToClassList("project-settings-title-bar");
            titleLabel.AddToClassList("project-settings-title-bar__label");

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var container = new VisualElement();
            container.AddToClassList("variant-type");
            root.Add(container);

            var minLongCount = 2;
            var maxLongCount = VariantData.MAX_LONG_COUNT;
            var longCount = VariantData.BYTE_COUNT / VariantData.SIZE_OF_LONG;
            var intCount = longCount * 2;
            var byteCount = longCount * 8;

            var helpBox = new HelpBox {
                messageType = HelpBoxMessageType.Info,
                text = "The size of Variant type for this project is currently defined by one of these scripting symbols:\n" +
                $"- {string.Format(VariantTypeSettings.LONG_SYMBOL_FORMAT, longCount)}\n" +
                $"- {string.Format(VariantTypeSettings.INT_SYMBOL_FORMAT, intCount)}\n" +
                $"- {string.Format(VariantTypeSettings.BYTE_SYMBOL_FORMAT, byteCount)}",
            };

            var sizeContainer = new VisualElement();
            sizeContainer.AddToClassList("size-container");

            var sizeLabel = new Label {
                text = "The amount of 64-bit integers that can fit into a Variant",
            };

            sizeLabel.AddToClassList("size-label");

            var sizeSlider = new SliderInt(minLongCount, maxLongCount) {
                value = longCount,
                showInputField = true,
            };

            sizeSlider.AddToClassList("size-slider");
            sizeSlider.bindingPath = nameof(VariantTypeSettings._variantSize);

            var sizeUnitLabel = _unitLabel = new Label {
                text = string.Format(UNIT_LABEL, longCount, intCount, byteCount),
            };

            sizeUnitLabel.AddToClassList("size-unit-label");

            var button = new Button {
                text = $"Apply to {UserBuildAPI.ActiveNamedBuildTarget.TargetName} build group",
            };

            button.AddToClassList("apply-button");
            button.clicked += ApplyButton_Clicked;

            sizeContainer.Add(sizeLabel);
            sizeContainer.Add(sizeSlider);
            sizeContainer.Add(sizeUnitLabel);

            container.Add(helpBox);
            container.Add(sizeContainer);
            container.Add(button);

            sizeSlider.Bind(serializedSettings);
            sizeSlider.RegisterValueChangedCallback(SliderValueChanged);
        }

        public void Update()
        {
            // Only invoke `OtherValueUpdated` once if possible,
            // because it is going to invoke saving asset to disk.
            if (_otherValueUpdated == false)
            {
                return;
            }

            var time = EditorApplication.timeSinceStartup - _markTime;

            if (time >= SAVE_DELAY)
            {
                Save();
            }
        }

        public void Save()
        {
            if (_otherValueUpdated)
            {
                _otherValueUpdated = false;

                var serializedObject = _serializedSettings;

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                EditorUtility.SetDirty(serializedObject.targetObject);
                AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
            }
        }

        private void ApplyButton_Clicked()
        {
            _settings.ApplyVariantSize();
        }

        private void SliderValueChanged(ChangeEvent<int> evt)
        {
            if (evt.newValue == evt.previousValue)
            {
                return;
            }

            var longCount = evt.newValue;
            var intCount = longCount * 2;
            var byteCount = longCount * 8;

            _unitLabel.text = string.Format(UNIT_LABEL, longCount, intCount, byteCount);

            OnValueUpdated();
        }

        private void OnValueUpdated()
        {
            _markTime = EditorApplication.timeSinceStartup;
            _otherValueUpdated = true;
        }
    }
}
