using System.ComponentModel;
using System.Runtime.CompilerServices;
using EncosyTower.Data;
using EncosyTower.Data.Authoring;
using EncosyTower.Databases;
using EncosyTower.StringIds;

namespace EncosyTower.Samples.Data
{
    [DataTableAsset]
    public sealed partial class EnemyTableAsset : DataTableAssetBase<EntityUid, EnemyData> { }

    [Data]
    public partial struct EnemyData
    {
        [DataProperty(typeof(EntityUidData))]
        public readonly EntityUid Id => Get_Id();

        [DataProperty(typeof(uint)), DataManualAuthoring]
        public readonly StringId Name => Get_Name();

        [DataProperty] public readonly EnemyType Type => Get_Type();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId Convert(uint value)
            => UIntToStringIdConverter.Convert(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Convert(StringId value)
            => StringIdToUIntConverter.Convert(value);
    }

    public enum EnemyType : byte
    {
        [Description("N")] Normal = 0,
        [Description("E")] Elite = 1,
        [Description("B")] Boss = 2,
    }
}
