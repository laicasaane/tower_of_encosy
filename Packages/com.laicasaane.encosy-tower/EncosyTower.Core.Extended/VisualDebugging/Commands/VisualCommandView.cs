using System;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Collections;
using EncosyTower.Initialization;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace EncosyTower.VisualDebugging.Commands
{
    public class VisualCommandView : VisualElement
    {
        public static readonly string UssClassName = "visual-command";
        public static readonly string HeaderUssClassName = $"{UssClassName}__header";
        public static readonly string HeaderLabelUssClassName = $"{HeaderUssClassName}-label";
        public static readonly string ExecuteButtonUssClassName = $"{HeaderUssClassName}-button__execute";
        public static readonly string ExecuteIconUssClassName = $"{ExecuteButtonUssClassName}-icon";
        public static readonly string ContentUssClassName = $"{UssClassName}__content";

        public event Action<VisualCommandView> Clicked;

        private readonly Label _label;
        private readonly VisualElement _container;

        public VisualCommandView()
        {
            AddToClassList(UssClassName);

            var header = new VisualElement();
            header.AddToClassList(HeaderUssClassName);
            Add(header);

            var label = _label = new Label();
            label.AddToClassList(HeaderLabelUssClassName);
            header.Add(label);

            var button = new Button();
            button.clicked += OnClicked;
            button.AddToClassList(ExecuteButtonUssClassName);
            header.Add(button);

            var icon = new Image();
            icon.AddToClassList(ExecuteIconUssClassName);
            button.Add(icon);

            var content = new ScrollView();
            content.AddToClassList(ContentUssClassName);
            Add(content);

            _container = content.Q("unity-content-container");
        }

        public VisualElement Container => _container;

        public void SetLabel(string value)
        {
            _label.text = value;
        }

        private void OnClicked()
        {
            Clicked?.Invoke(this);
        }
    }

    public class VisualCommandViewController
    {
        private readonly VisualCommandView _view;
        private readonly ObjectPool<VisualPropertyView> _propertyViewPool;
        private readonly FasterList<VisualPropertyView> _propertyViews;

        private VisualCommandData _data;

        public VisualCommandViewController(
              [NotNull] VisualCommandView view
            , [NotNull] ObjectPool<VisualPropertyView> propertyViewPool
        )
        {
            _view = view;
            _view.userData = this;
            _view.Clicked += OnClicked;

            _propertyViewPool = propertyViewPool;
            _propertyViews = new FasterList<VisualPropertyView>();
        }

        public void Initialize([NotNull] VisualCommandData data)
        {
            _data = data;
            _view.name = data.Name;
            _view.SetLabel(data.Label);

            PopulateProperties();

            if (data?.Command is IInitializable initializer)
            {
                initializer.Initialize();
            }
        }

        public void Deinitialize()
        {
            _data = null;
            ClearProperties();
        }

        private void PopulateProperties()
        {
            if (_data?.Properties is not { } properties
                || _data?.Command is not { } command
            )
            {
                return;
            }

            var views = _propertyViews;
            var propertyViewPool = _propertyViewPool;
            var container = _view.Container;

            foreach (var property in properties)
            {
                var view = propertyViewPool.Get();

                if (view.userData is not VisualPropertyViewController controller)
                {
                    controller = new VisualPropertyViewController(view);
                }

                controller.Initialize(property, command);
                container.Add(view);
                views.Add(view);
            }
        }

        private void ClearProperties()
        {
            var container = _view.Container;
            container.Clear();

            var views = _propertyViews;
            var viewPool = _propertyViewPool;

            foreach (var view in views)
            {
                viewPool.Release(view);
            }

            views.Clear();
        }

        private void OnClicked(VisualCommandView _)
        {
            _data?.Command?.Execute();
        }
    }

    public static class VisualCommandAPI
    {
        public static VisualCommandView CreateView()
            => new();

        public static void ReleaseView(VisualCommandView view)
        {
            if (view.userData is VisualCommandViewController controller)
            {
                controller.Deinitialize();
            }
        }
    }
}
