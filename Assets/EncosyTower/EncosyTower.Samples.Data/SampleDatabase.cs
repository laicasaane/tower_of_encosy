using EncosyTower.Databases;
using EncosyTower.Naming;

namespace EncosyTower.Samples.Data
{
    [Database(NamingStrategy.SnakeCase, AssetName = $"{nameof(SampleDatabase)}Asset")]
    public readonly partial struct SampleDatabase
    {
        [Table] public readonly HeroTableAsset Heroes => Get_Heroes();

        [Table] public readonly EnemyTableAsset Enemies => Get_Enemies();
    }
}
