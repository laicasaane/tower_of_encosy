using System;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Canvas", "UI")]
    public sealed partial class CanvasBinder : MonoBinder<Canvas>
    {
    }

    [Serializable]
    [Label("Camera", "Canvas")]
    public sealed partial class CanvasBindingCamera : MonoBindingProperty<Canvas>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetCamera(Camera value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].worldCamera = value;
            }
        }
    }

    [Serializable]
    [Label("Override Sorting", "Canvas")]
    public sealed partial class CanvasBindingOverrideSorting : MonoBindingProperty<Canvas>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetOverrideSorting(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].overrideSorting = value;
            }
        }
    }

    [Serializable]
    [Label("Sorting Layer Id", "Canvas")]
    public sealed partial class CanvasBindingSortingLayerId : MonoBindingProperty<Canvas>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSortingLayerId(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sortingLayerID = value;
            }
        }
    }

    [Serializable]
    [Label("Sorting Layer Name", "Canvas")]
    public sealed partial class CanvasBindingSortingLayerName : MonoBindingProperty<Canvas>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSortingLayerName(string value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sortingLayerName = value;
            }
        }
    }

    [Serializable]
    [Label("Sorting Order In Layer", "Canvas")]
    public sealed partial class CanvasBindingSortingOrderInLayer : MonoBindingProperty<Canvas>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetSortingOrderInLayer(int value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].sortingOrder = value;
            }
        }
    }

}
