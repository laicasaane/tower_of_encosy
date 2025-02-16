using EncosyTower.Data;

namespace EncosyTower.Samples.Data
{
    public sealed partial class EnemyTableAsset
        : DataTableAsset<EntityUidData, EnemyData, EntityUid>
        , IDataTableAsset
    { }

    public partial struct EnemyData : IData
    {
        [DataProperty] public readonly EntityUidData Id => Get_Id();

        [DataProperty] public readonly string Name => Get_Name();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();
    }
}
