using System;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Mvvm.ViewBinding;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Binders.Unity
{
    [Serializable]
    [Label("Audio Source")]
    public sealed partial class AudioSourceBinder : MonoBinder<AudioSource>
    {
    }

    [Serializable]
    [Label("Loop", "Audio Source")]
    public sealed partial class AudioSourceBindingLoop : MonoBindingProperty<AudioSource>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetLoop(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].loop = value;
            }
        }
    }

    [Serializable]
    [Label("Mute", "Audio Source")]
    public sealed partial class AudioSourceBindingMute : MonoBindingProperty<AudioSource>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetMute(bool value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].mute = value;
            }
        }
    }

    [Serializable]
    [Label("Volume", "Audio Source")]
    public sealed partial class AudioSourceBindingVolume : MonoBindingProperty<AudioSource>, IBinder
    {
        [BindingProperty]
        [field: HideInInspector]
        private void SetVolume(float value)
        {
            var targets = Targets;
            var length = targets.Length;

            for (var i = 0; i < length; i++)
            {
                targets[i].volume = value;
            }
        }
    }
}
