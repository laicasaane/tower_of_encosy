#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable, Binder]
    [Label("Canvas Group", "UI")]
    public sealed partial class CanvasGroupBinder : MonoBinder<CanvasGroup>
    {
    }

    [Serializable, Binder]
    [Label("Alpha", "Canvas Group")]
    public sealed partial class CanvasGroupBindingAlpha : MonoBindingProperty<CanvasGroup>
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

    [Serializable, Binder]
    [Label("Blocks Raycasts", "Canvas Group")]
    public sealed partial class CanvasGroupBindingBlocksRaycasts : MonoBindingProperty<CanvasGroup>
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

    [Serializable, Binder]
    [Label("Interactable", "Canvas Group")]
    public sealed partial class CanvasGroupBindingInteractable : MonoBindingProperty<CanvasGroup>
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
