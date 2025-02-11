#if UNITY_EDITOR

using EncosyTower.Modules.Data.Authoring;

namespace EncosyTower.Samples.Data.Authoring
{
    [Database(AssetName = "DatabaseAsset")]
    public partial class SampleDatabase
    {
        [Table] public HeroTableAsset Heroes { get; }

        [Table] public EnemyTableAsset Enemies { get; }
    }
}

#endif
