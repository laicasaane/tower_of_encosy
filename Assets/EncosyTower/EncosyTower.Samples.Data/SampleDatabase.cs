using EncosyTower.Databases.Authoring;
using EncosyTower.Naming;

namespace EncosyTower.Samples.Data
{
    [Database(NamingStrategy.SnakeCase, AssetName = $"{nameof(SampleDatabase)}Asset")]
    public readonly partial struct SampleDatabase
    {
        [Table] public HeroTableAsset Heroes { get; }

        [Table] public EnemyTableAsset Enemies { get; }
    }
}
