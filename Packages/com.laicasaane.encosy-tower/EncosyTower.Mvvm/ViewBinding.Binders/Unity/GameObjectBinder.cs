#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable, Binder]
    [Label("GameObject")]
    public sealed partial class GameObjectBinder : MonoBinder<GameObject>
    {
    }

    [Serializable, Binder]
    [Label("Active Self", "GameObject")]
    public sealed partial class GameObjectBindingActiveSelf : MonoBindingProperty<GameObject>
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
