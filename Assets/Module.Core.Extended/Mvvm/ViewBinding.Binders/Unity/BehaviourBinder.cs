using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable]
    [Label("Behaviour")]
    public sealed partial class BehaviourBinder : MonoBinder<Behaviour>
    {
    }

    [Serializable]
    [Label("Enabled", "Behaviour")]
    public sealed partial class BehaviourBindingEnabled : MonoBindingProperty<Behaviour>, IBinder
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
