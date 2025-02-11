using System;
using EncosyTower.Modules.Data;

namespace EncosyTower.Samples.Data
{
    public sealed partial class HeroTableAsset
        : DataTableAsset<EntityIdData, HeroData, EntityId>
        , IDataTableAsset { }

    public partial struct HeroData : IData
    {
        [DataProperty] public readonly EntityIdData Id => Get_Id();

        [DataProperty] public readonly string Name => Get_Name();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();

        [DataProperty] public readonly ReadOnlyMemory<EntityStatMultiplierData> Multipliers => Get_Multipliers();
    }
}
