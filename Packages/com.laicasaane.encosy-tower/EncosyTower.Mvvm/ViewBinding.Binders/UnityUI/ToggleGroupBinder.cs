#if UNITY_UGUI

#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Toggle Group", "UI")]
    public sealed partial class ToggleGroupBinder : MonoBinder<ToggleGroup>
    {
    }

    [Serializable]
    [Label("Allow Switch Off", "Toggle Group")]
    public sealed partial class ToggleGroupBindingAllowSwitchOff : MonoBindingProperty<ToggleGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAllowSwitchOff(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].allowSwitchOff = value;
            }
        }
    }
}

#endif
