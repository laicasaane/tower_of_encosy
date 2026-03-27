using EncosyTower.Data;
using EncosyTower.Databases;

namespace EncosyTower.Samples.Data
{
    [DataTableAsset]
    public sealed partial class EnemyTableAsset : DataTableAsset<EntityUidData, EnemyData, EntityUid> { }

    [Data]
    public partial struct EnemyData
    {
        [DataProperty] public readonly EntityUidData Id => Get_Id();

        [DataProperty] public readonly string Name => Get_Name();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();
    }
}
