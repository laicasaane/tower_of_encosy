using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;

namespace EncosyTower.VisualCommands
{
    public class VisualDirectoryView : RadioButton
    {
        public static readonly string UssClassName = "visual-directory";
        public static readonly string LabelUssClassName = $"{UssClassName}__label";

        public event Action<VisualDirectoryView> Selected;
        public event Action<VisualDirectoryView> Deselected;

        private readonly Label _label;

        public VisualDirectoryView() : this(string.Empty)
        { }

        public VisualDirectoryView(string label) : base(string.Empty)
        {
            AddToClassList(UssClassName);

            var labelField = _label = new Label(label);
            labelField.AddToClassList(LabelUssClassName);

            var checkmark = this.Q("unity-checkmark");
            checkmark.Add(labelField);

            UpdateCheckmark();

            this.RegisterValueChangedCallback(OnValueChanged);
        }

        public override bool value
        {
            get
            {
                return base.value;
            }
            set
            {
                base.value = value;
                UpdateCheckmark();
            }
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateCheckmark();
        }

        public void SetLabel(string value)
        {
            _label.text = value;
        }

        private void UpdateCheckmark()
        {
            m_CheckMark.style.display = DisplayStyle.Flex;
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                Selected?.Invoke(this);
            }
            else
            {
                Deselected?.Invoke(this);
            }
        }
    }

    public class VisualDirectoryViewController
    {
        private readonly VisualDirectoryView _view;

        private VisualDirectoryData _data;
        private Action<VisualDirectoryData> _selected;

        public VisualDirectoryViewController([NotNull] VisualDirectoryView view)
        {
            _view = view;
            _view.userData = this;
            _view.Selected += OnSelected;
        }

        public void Initialize(
              [NotNull] VisualDirectoryData data
            , [NotNull] Action<VisualDirectoryData> selected
        )
        {
            _data = data;
            _selected = selected;
            _view.name = data.Name;
            _view.SetLabel(data.Label);
        }

        public void Deinitialize()
        {
            _data = null;
            _selected = null;
        }

        private void OnSelected(VisualDirectoryView _)
            => _selected?.Invoke(_data);
    }

    public static class VisualDirectoryAPI
    {
        public static VisualDirectoryView CreateView()
            => new();

        public static void ReleaseView(VisualDirectoryView view)
        {
            if (view.userData is VisualDirectoryViewController controller)
            {
                controller.Deinitialize();
            }
        }
    }
}
