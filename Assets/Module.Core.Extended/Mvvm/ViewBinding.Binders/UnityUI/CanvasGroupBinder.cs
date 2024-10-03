using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Canvas Group", "UI")]
    public sealed partial class CanvasGroupBinder : MonoBinder<CanvasGroup>
    {
    }

    [Serializable]
    [Label("Alpha", "Canvas Group")]
    public sealed partial class CanvasGroupBindingAlpha : MonoBindingProperty<CanvasGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetAlpha(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].alpha = value;
            }
        }
    }

    [Serializable]
    [Label("Blocks Raycasts", "Canvas Group")]
    public sealed partial class CanvasGroupBindingBlocksRaycasts : MonoBindingProperty<CanvasGroup>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBlocksRaycasts(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].blocksRaycasts = value;
            }
        }
    }

    [Serializable]
    [Label("Interactable", "Canvas Group")]
    public sealed partial class CanvasGroupBindingInteractable : MonoBindingProperty<CanvasGroup>, IBinder
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
