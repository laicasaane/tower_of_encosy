using System;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
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
