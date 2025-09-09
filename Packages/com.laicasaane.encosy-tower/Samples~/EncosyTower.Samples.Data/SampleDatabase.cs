namespace EncosyTower.Samples.Data
{
    using EncosyTower.Databases;
    using EncosyTower.Naming;

    [Database(NamingStrategy.SnakeCase, AssetName = $"{nameof(SampleDatabase)}Asset")]
    public readonly partial struct SampleDatabase
    {
        [Table] public readonly HeroTableAsset Heroes => Get_Heroes();

        [Table] public readonly EnemyTableAsset Enemies => Get_Enemies();
    }
}

#if UNITY_EDITOR && BAKING_SHEET
namespace EncosyTower.Samples.DatabaseAuthoring
{
    using EncosyTower.Databases.Authoring;
    using EncosyTower.Samples.Data;

    [AuthorDatabase(typeof(SampleDatabase))]
    public readonly partial struct SampleDatabaseAuthoring { }
}
#endif
