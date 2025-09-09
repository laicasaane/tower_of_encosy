using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.VisualDebugging.Commands.Bindings;
using UnityEngine.UIElements;

namespace EncosyTower.VisualDebugging.Commands
{
    public class VisualOptionsView : Button
    {
        public static readonly string UssClassName = "visual-options";
        public static readonly string IconUssClassName = $"{UssClassName}__icon";

        private readonly VisualOptionMenu _menu;

        public VisualOptionsView([NotNull] VisualOptionMenu menu)
        {
            _menu = menu;

            AddToClassList(UssClassName);

            var icon = new Image();
            icon.AddToClassList(IconUssClassName);
            Add(icon);

            clicked += OnClicked;
        }

        public event Action<VisualOption> Selected
        {
            add => _menu.Selected += value;
            remove => _menu.Selected -= value;
        }

        private void OnClicked()
        {
            var menu = _menu.Rebuild();
            menu.DropDown(worldBound, this, false);
        }
    }

    public class VisualOptionMenu
    {
        private readonly IVisualCommand _command;
        private readonly MethodInfo _optionsGetter;
        private readonly GenericDropdownMenu _menu;

        public event Action<VisualOption> Selected;

        public VisualOptionMenu(
              [NotNull] IVisualCommand command
            , [NotNull] MethodInfo optionsGetter
            , bool isDataStatic

        )
        {
            _command = command;
            _optionsGetter = optionsGetter;

            if (isDataStatic == false)
            {
                return;
            }

            _menu = Create();
        }

        public GenericDropdownMenu Rebuild()
        {
            if (_menu is not null)
            {
                return _menu;
            }

            return Create();
        }

        private GenericDropdownMenu Create()
        {
            var menu = new GenericDropdownMenu();
            object obj = _optionsGetter.IsStatic ? null : _command;

            if (_optionsGetter.Invoke(obj, null) is IReadOnlyList<string> options
                && options.Count > 0
            )
            {
                var length = options.Count;

                for (var i = 0; i < length; i++)
                {
                    var optionStr = options[i];
                    var stateOption = new StateOption(this, new VisualOption(i, optionStr));
                    menu.AddItem(optionStr, false, OnSelected, stateOption);
                }
            }

            return menu;
        }

        private static void OnSelected(object data)
        {
            if (data is StateOption option)
            {
                option.State.Selected?.Invoke(option.Option);
            }
        }

        private record StateOption(VisualOptionMenu State, VisualOption Option);
    }

    internal partial class VisualOptionsViewBinding : VisualPropertyBinding, IBinder
    {
        private readonly VisualOptionsView _view;

        public VisualOptionsViewBinding([NotNull] VisualOptionsView view)
        {
            _view = view;
            _view.userData = this;
            _view.Selected += OnValueChanged;
        }

        [BindingCommand]
        partial void OnValueChanged(VisualOption value);
    }
}
