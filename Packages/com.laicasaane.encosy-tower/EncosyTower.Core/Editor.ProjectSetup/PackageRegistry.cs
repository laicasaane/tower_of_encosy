#if UNITY_EDITOR

using EncosyTower.Core;

namespace EncosyTower.Editor.ProjectSetup
{
    /// <summary>
    /// The location/service where the package is hosted.
    /// </summary>
    [ApiForEditor]
    public enum PackageRegistry
    {
        [ApiForEditor] Unity,
        [ApiForEditor] OpenUpm,
        [ApiForEditor] GitUrl,
    }
}

#endif
