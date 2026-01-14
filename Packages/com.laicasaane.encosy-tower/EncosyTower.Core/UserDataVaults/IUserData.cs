#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.UserDataVaults
{
    public interface IUserData
    {
        string Id { get; set; }

        string Version { get; set; }
    }
}

#endif
