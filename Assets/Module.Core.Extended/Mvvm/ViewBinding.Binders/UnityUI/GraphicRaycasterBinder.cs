#if UNITY_UGUI

using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.UnityUI
{
    [Serializable]
    [Label("Graphic Raycaster", "UI")]
    public sealed partial class GraphicRaycasterBinder : MonoBinder<GraphicRaycaster>
    {
    }

    [Serializable]
    [Label("Ignore Reversed Graphics", "Graphic Raycaster")]
    public sealed partial class GraphicRaycasterBindingIgnoreReversedGraphics : MonoBindingProperty<GraphicRaycaster>, IBinder
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

    [Serializable]
    [Label("Blocking Objects", "Graphic Raycaster")]
    public sealed partial class GraphicRaycasterBindingBlockingObjects : MonoBindingProperty<GraphicRaycaster>, IBinder
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

    [Serializable]
    [Label("Blocking Mask", "Graphic Raycaster")]
    public sealed partial class GraphicRaycasterBindingBlockingMask : MonoBindingProperty<GraphicRaycaster>, IBinder
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
