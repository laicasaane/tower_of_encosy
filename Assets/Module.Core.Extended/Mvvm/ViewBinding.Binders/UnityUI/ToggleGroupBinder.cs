#if UNITY_UGUI

using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine.UI;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI
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
