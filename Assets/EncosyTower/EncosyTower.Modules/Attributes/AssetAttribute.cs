using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Modules
{
    [AttributeUsage(AttributeTargets.Field)]
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
