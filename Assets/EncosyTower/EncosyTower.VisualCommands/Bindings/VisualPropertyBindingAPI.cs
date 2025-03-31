using System;
using System.Collections.Generic;
using EncosyTower.Mvvm.ComponentModel;
using UnityEngine.UIElements;

namespace EncosyTower.VisualCommands.Bindings
{
    internal static class VisualPropertyBindingAPI
    {
        private const string BINDING_PROPERTY_NAME = "SetValue";
        private const string BINDING_COMMAND_NAME = "OnValueChanged";

        public static void Create(
              IVisualCommand command
            , VisualPropertyType type
            , Enum defaultEnumValue
            , string name
            , string propertyName
            , VisualOptionsData optionsData
            , VisualElement root
        )
        {
            if (string.IsNullOrEmpty(propertyName)
                || command is not IObservableObject context
                || root is null
            )
            {
                return;
            }

            name = $"{name}-field";

            VisualPropertyBinding binding = null;

            switch (type)
            {
                case VisualPropertyType.Bool:
                {
                    var view = new Toggle { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingToggle();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Bounds:
                {
                    var view = new BoundsField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingBounds();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.BoundsInt:
                {
                    var view = new BoundsIntField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingBoundsInt();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.DateTime:
                {
                    var view = new TextField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingDateTime();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Double:
                {
                    var view = new DoubleField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingDouble();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Enum:
                {
                    var view = new EnumField(defaultEnumValue) { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingEnum();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Float:
                {
                    var view = new FloatField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingFloat();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Integer:
                {
                    var view = new IntegerField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingInteger();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Long:
                {
                    var view = new LongField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingLong();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Rect:
                {
                    var view = new RectField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingRect();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.RectInt:
                {
                    var view = new RectIntField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingRectInt();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.String:
                {
                    var view = new TextField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingString();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.UnsignedInteger:
                {
                    var view = new UnsignedIntegerField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingUnsignedInteger();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.UnsignedLong:
                {
                    var view = new UnsignedLongField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingUnsignedLong();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Vector2:
                {
                    var view = new Vector2Field { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingVector2();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Vector2Int:
                {
                    var view = new Vector2IntField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingVector2Int();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Vector3:
                {
                    var view = new Vector3Field { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingVector3();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Vector3Int:
                {
                    var view = new Vector3IntField { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingVector3Int();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }

                case VisualPropertyType.Vector4:
                {
                    var view = new Vector4Field { name = name };
                    root.Add(view);

                    var viewBinding = new VisualPropertyBindingVector4();
                    viewBinding.SetTarget(view);

                    view.userData = binding = viewBinding;
                    break;
                }
            }

            if (binding is null)
            {
                return;
            }

            binding.SetContext(context);
            binding.SetTargetPropertyName(BINDING_PROPERTY_NAME, propertyName);
            binding.SetTargetCommandName(BINDING_COMMAND_NAME, $"Set{propertyName}Command");
            binding.StartListening();

            CreateOptions(command, context, optionsData, root);
        }

        private static void CreateOptions(
              IVisualCommand command
            , IObservableObject context
            , VisualOptionsData data
            , VisualElement root
        )
        {
            if (data is null)
            {
                return;
            }

            var (optionsGetter, commandName, isDataStatic) = data;
            var menu = new VisualOptionMenu(command, optionsGetter, isDataStatic);
            var view = new VisualOptionsView(menu);
            root.Add(view);

            var binding = new VisualOptionsViewBinding(view);
            binding.SetContext(context);
            binding.SetTargetCommandName(BINDING_COMMAND_NAME, commandName);
            binding.StartListening();
        }
    }
}
