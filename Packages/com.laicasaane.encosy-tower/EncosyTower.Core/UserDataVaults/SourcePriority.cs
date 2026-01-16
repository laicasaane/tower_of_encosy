#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.UserDataVaults
{
    public enum SourcePriority : byte
    {
        CloudThenDevice,
        DeviceThenCloud,
        OnlyCloud,
        OnlyDevice,
    }
}

#endif
