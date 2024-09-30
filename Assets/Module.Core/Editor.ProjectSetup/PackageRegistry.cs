#if UNITY_EDITOR

namespace Module.Core.Editor.ProjectSetup
{
    /// <summary>
    /// The location/service where the package is hosted.
    /// </summary>
    public enum PackageRegistry
    {
        Unity,
        OpenUpm,
        GitUrl,
    }
}

#endif
