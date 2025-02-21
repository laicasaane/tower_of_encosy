#if UNITY_UGUI

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Selectable", "UI")]
    public sealed partial class SelectableBinder : MonoBinder<Selectable>
    {
    }

    [Serializable]
    [Label("Interactable", "Selectable")]
    public sealed partial class SelectableBindingInteractable : MonoBindingProperty<Selectable>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetInteractable(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].interactable = value;
            }
        }
    }
}

#endif
