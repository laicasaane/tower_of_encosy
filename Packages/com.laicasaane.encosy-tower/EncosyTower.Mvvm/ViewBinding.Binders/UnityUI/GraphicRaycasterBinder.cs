#if UNITY_UGUI

#pragma warning disable CS0657, IDE0005

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable, Binder]
    [Label("Graphic Raycaster", "UI")]
    public sealed partial class GraphicRaycasterBinder : MonoBinder<GraphicRaycaster>
    {
    }

    [Serializable, Binder]
    [Label("Ignore Reversed Graphics", "Graphic Raycaster")]
    public sealed partial class GraphicRaycasterBindingIgnoreReversedGraphics : MonoBindingProperty<GraphicRaycaster>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetIgnoreReversedGraphics(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].ignoreReversedGraphics = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Blocking Objects", "Graphic Raycaster")]
    public sealed partial class GraphicRaycasterBindingBlockingObjects : MonoBindingProperty<GraphicRaycaster>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBlockingObjects(GraphicRaycaster.BlockingObjects value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].blockingObjects = value;
            }
        }
    }

    [Serializable, Binder]
    [Label("Blocking Mask", "Graphic Raycaster")]
    public sealed partial class GraphicRaycasterBindingBlockingMask : MonoBindingProperty<GraphicRaycaster>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetBlockingMask(LayerMask value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].blockingMask = value;
            }
        }
    }
}

#endif
