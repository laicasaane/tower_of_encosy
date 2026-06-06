using System.ComponentModel;
using EncosyTower.Data;
using EncosyTower.Databases;

namespace EncosyTower.Samples.Data
{
    [DataTableAsset]
    public sealed partial class EnemyTableAsset : DataTableAssetBase<EntityUid, EnemyData> { }

    [Data]
    public partial struct EnemyData
    {
        [DataProperty(typeof(EntityUidData))]
        public readonly EntityUid Id => Get_Id();

        [DataProperty] public readonly string Name => Get_Name();

        [DataProperty] public readonly EnemyType Type => Get_Type();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();
    }

    public enum EnemyType : byte
    {
        [Description("N")] Normal = 0,
        [Description("E")] Elite = 1,
        [Description("B")] Boss = 2,
    }
}
