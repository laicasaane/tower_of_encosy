#if UNITY_EDITOR

using EncosyTower.Modules.Data.Authoring;

namespace EncosyTower.Samples.Data.Authoring
{
    [Database(NamingStrategy.SnakeCase, AssetName = "DatabaseAsset")]
    public partial class SampleDatabase
    {
        [Table] public HeroTableAsset heroes;
        [Table] public EnemyTableAsset enemies;
    }
}

#endif
