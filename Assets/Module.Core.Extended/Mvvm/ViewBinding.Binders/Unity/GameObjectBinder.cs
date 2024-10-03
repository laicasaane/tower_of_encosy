using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable]
    [Label("GameObject")]
    public sealed partial class GameObjectBinder : MonoBinder<GameObject>
    {
    }

    [Serializable]
    [Label("Active Self", "GameObject")]
    public sealed partial class GameObjectBindingActiveSelf : MonoBindingProperty<GameObject>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetActive(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].SetActive(value);
            }
        }
    }
}
