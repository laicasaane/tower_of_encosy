using System;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    [Serializable]
    public sealed class MonoViewSettings
    {
        public InitializationMethod initializeOn = InitializationMethod.Awake;
        public bool intializeAsync = true;
        public bool startListeningOnInitialized = true;
    }

    public enum InitializationMethod
    {
        Awake,
        Start,
        Manual,
    }
}