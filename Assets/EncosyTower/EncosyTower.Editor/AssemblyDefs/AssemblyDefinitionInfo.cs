#if UNITY_EDITOR

using System;

namespace EncosyTower.Editor.AssemblyDefs
{
    [Serializable]
    public class AssemblyDefinitionInfo
    {
        public string name = string.Empty;
        public string rootNamespace = string.Empty;
        public string[] references = Array.Empty<string>();
        public string[] includePlatforms = Array.Empty<string>();
        public string[] excludePlatforms = Array.Empty<string>();
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public string[] precompiledReferences = Array.Empty<string>();
        public bool autoReferenced;
        public string[] defineConstraints = Array.Empty<string>();
        public VersionDefineInfo[] versionDefines = Array.Empty<VersionDefineInfo>();
        public bool noEngineReferences;
    }

    [Serializable]
    public class VersionDefineInfo
    {
        public string name = string.Empty;
        public string expression = string.Empty;
        public string define = string.Empty;
    }
}

#endif
