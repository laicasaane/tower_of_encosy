using System.Diagnostics.CodeAnalysis;
using EncosyTower.VisualDebugging.Commands.Bindings;
using UnityEngine.UIElements;

namespace EncosyTower.VisualDebugging.Commands
{
    public class VisualPropertyView : VisualElement
    {
        public static readonly string UssClassName = "visual-property";
        public static readonly string LabelUssClassName = $"{UssClassName}__label";
        public static readonly string ContentUssClassName = $"{UssClassName}__content";

        private readonly Label _label;
        private readonly VisualElement _content;

        public VisualPropertyView()
        {
            AddToClassList(UssClassName);

            var label = _label = new Label();
            label.AddToClassList(LabelUssClassName);
            Add(label);

            var content = _content = new VisualElement();
            content.AddToClassList(ContentUssClassName);
            Add(content);
        }

        public VisualElement Content => _content;

        public void SetLabel(string value)
        {
            _label.text = value;
        }
    }

    public class VisualPropertyViewController
    {
        private readonly VisualPropertyView _view;

        private VisualPropertyData _data;

        public VisualPropertyViewController([NotNull] VisualPropertyView view)
        {
            _view = view;
            _view.userData = this;
        }

        public void Initialize(
              [NotNull] VisualPropertyData data
            , [NotNull] IVisualCommand command
        )
        {
            _data = data;
            _view.name = data.Name;
            _view.SetLabel(data.Label);

            VisualPropertyBindingAPI.Create(
                  command
                , data.Type
                , data.DefaultEnumValue
                , data.Name
                , data.PropertyName
                , data.Options
                , _view.Content
            );
        }

        public void Deinitialize()
        {
            _data = null;
            _view.Content.Clear();
        }
    }

    public static class VisualPropertyAPI
    {
        public static VisualPropertyView CreateView()
            => new();

        public static void ReleaseView(VisualPropertyView view)
        {
            if (view.userData is VisualPropertyViewController controller)
            {
                controller.Deinitialize();
            }
        }
    }
}
