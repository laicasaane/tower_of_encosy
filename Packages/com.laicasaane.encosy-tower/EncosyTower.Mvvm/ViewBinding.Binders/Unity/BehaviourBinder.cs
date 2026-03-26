#pragma warning disable CS0657

using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding.Components;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable, Binder]
    [Label("Behaviour")]
    public sealed partial class BehaviourBinder : MonoBinder<Behaviour>
    {
    }

    [Serializable, Binder]
    [Label("Enabled", "Behaviour")]
    public sealed partial class BehaviourBindingEnabled : MonoBindingProperty<Behaviour>
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetEnabled(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].enabled = value;
            }
        }
    }
}
