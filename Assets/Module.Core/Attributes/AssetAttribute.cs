using System;
using System.Diagnostics.CodeAnalysis;

namespace Module.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class AssetAttribute : Attribute
    {
        public string Guid { get; }

        public string Path { get; }

        public Type MainAssetType { get; }

        public AssetAttribute([NotNull] string guid, [NotNull] string path, [NotNull] Type mainAssetType)
        {
            Guid = guid;
            Path = path;
            MainAssetType = mainAssetType;
        }
    }
}
