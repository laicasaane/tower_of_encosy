using System;
using UnityEngine.Serialization;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    [Serializable]
    public sealed class MonoViewSettings
    {
        public InitializationMethod initializeOn = InitializationMethod.Awake;
        public bool initializeAsync = true;
        public bool startListeningOnInitialized = true;
    }

    public enum InitializationMethod
    {
        Awake,
        Start,
        Manual,
    }
}
