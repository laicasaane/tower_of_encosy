using System;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.ViewBinding;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.VisualCommands.Bindings
{
    internal abstract partial class VisualPropertyBinding : IBinder
    {
        public void SetContext(IObservableObject context)
        {
            StopListening();
            Context = context;
        }
    }

    internal abstract partial class VisualPropertyBinding<T> : VisualPropertyBinding, IBinder
        where T : VisualElement
    {
        protected T target;

        public void SetTarget(T target)
        {
            OnBeforeSetTarget();

            this.target = target;

            OnAfterSetTarget();
        }

        protected virtual void OnBeforeSetTarget() { }

        protected virtual void OnAfterSetTarget() { }
    }

    internal partial class VisualPropertyBindingToggle : VisualPropertyBinding<Toggle>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(bool value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(bool value);

        private void OnValueChanged(ChangeEvent<bool> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingBounds : VisualPropertyBinding<BoundsField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Bounds value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Bounds value);

        private void OnValueChanged(ChangeEvent<Bounds> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingBoundsInt : VisualPropertyBinding<BoundsIntField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(BoundsInt value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(BoundsInt value);

        private void OnValueChanged(ChangeEvent<BoundsInt> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingDateTime : VisualPropertyBinding<TextField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(DateTime value)
        {
            if (target is not null)
            {
                target.value = $"{value:s}";
            }
        }

        [BindingCommand]
        partial void OnValueChanged(DateTime value);

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            if (DateTime.TryParse(evt.newValue, out var value))
            {
                OnValueChanged(value);
            }
        }
    }

    internal partial class VisualPropertyBindingDouble : VisualPropertyBinding<DoubleField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(double value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(double value);

        private void OnValueChanged(ChangeEvent<double> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingEnum : VisualPropertyBinding<EnumField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Enum value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Enum value);

        private void OnValueChanged(ChangeEvent<Enum> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingFloat : VisualPropertyBinding<FloatField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(float value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(float value);

        private void OnValueChanged(ChangeEvent<float> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingInteger : VisualPropertyBinding<IntegerField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(int value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(int value);

        private void OnValueChanged(ChangeEvent<int> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingLong : VisualPropertyBinding<LongField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(long value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(long value);

        private void OnValueChanged(ChangeEvent<long> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingRect : VisualPropertyBinding<RectField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Rect value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Rect value);

        private void OnValueChanged(ChangeEvent<Rect> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingRectInt : VisualPropertyBinding<RectIntField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(RectInt value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(RectInt value);

        private void OnValueChanged(ChangeEvent<RectInt> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingString : VisualPropertyBinding<TextField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(string value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(string value);

        private void OnValueChanged(ChangeEvent<string> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingUnsignedInteger : VisualPropertyBinding<UnsignedIntegerField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(uint value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(uint value);

        private void OnValueChanged(ChangeEvent<uint> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingUnsignedLong : VisualPropertyBinding<UnsignedLongField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(ulong value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(ulong value);

        private void OnValueChanged(ChangeEvent<ulong> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingVector2 : VisualPropertyBinding<Vector2Field>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Vector2 value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Vector2 value);

        private void OnValueChanged(ChangeEvent<Vector2> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingVector2Int : VisualPropertyBinding<Vector2IntField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Vector2Int value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Vector2Int value);

        private void OnValueChanged(ChangeEvent<Vector2Int> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingVector3 : VisualPropertyBinding<Vector3Field>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Vector3 value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Vector3 value);

        private void OnValueChanged(ChangeEvent<Vector3> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingVector3Int : VisualPropertyBinding<Vector3IntField>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Vector3Int value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        partial void OnValueChanged(Vector3Int value);

        private void OnValueChanged(ChangeEvent<Vector3Int> evt)
            => OnValueChanged(evt.newValue);
    }

    internal partial class VisualPropertyBindingVector4 : VisualPropertyBinding<Vector4Field>, IBinder
    {
        protected override void OnBeforeSetTarget()
            => target?.UnregisterValueChangedCallback(OnValueChanged);

        protected override void OnAfterSetTarget()
            => target?.RegisterValueChangedCallback(OnValueChanged);

        [BindingProperty]
        private void SetValue(Vector4 value)
        {
            if (target is not null)
            {
                target.value = value;
            }
        }

        [BindingCommand]
        [field: HideInInspector]
        partial void OnValueChanged(Vector4 value);

        private void OnValueChanged(ChangeEvent<Vector4> evt)
            => OnValueChanged(evt.newValue);
    }
}
