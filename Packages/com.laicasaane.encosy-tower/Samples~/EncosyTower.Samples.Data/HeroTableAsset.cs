using System;
using EncosyTower.Data;
using EncosyTower.Data.Authoring;
using EncosyTower.Databases;
using EncosyTower.StringIds;

namespace EncosyTower.Samples.Data
{
    [DataTableAsset]
    public sealed partial class HeroTableAsset : DataTableAssetBase<EntityId, HeroData> { }

    [Data, DataMutable(DataMutableOptions.WithReadOnlyView)]
    public partial struct HeroData
    {
        [DataProperty(typeof(EntityIdData))]
        public readonly EntityId Id => Get_Id();

        [DataProperty(typeof(uint), typeof(StringIdValueConverter))]
        [DataManualAuthoring(typeof(string))]
        public readonly StringId Name => Get_Name();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();

        [DataProperty] public readonly ReadOnlyMemory<EntityStatMultiplierData> Multipliers => Get_Multipliers();

        public readonly override string ToString()
            => $"{_id} :: {_name} :: {_stat}";
    }
}
