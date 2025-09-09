using System;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
    [Serializable]
    public sealed class MonoViewSettings
    {
        public InitializationMethod initializeOn = InitializationMethod.Awake;
        public bool initializeAsync = true;
        public bool startAfterInitialization = true;
    }

    public enum InitializationMethod
    {
        Awake,
        Start,
        Manual,
    }
}
