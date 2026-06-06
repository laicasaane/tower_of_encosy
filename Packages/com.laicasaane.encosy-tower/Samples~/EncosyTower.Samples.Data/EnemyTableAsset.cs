using System;
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

        [DataProperty(typeof(uint), typeof(StringIdValueConverter))]
        [DataManualAuthoring(typeof(string))]
        public readonly StringId Name => Get_Name();

        [DataProperty] public readonly EnemyType Type => Get_Type();

        [DataProperty] public readonly EntityStatData Stat => Get_Stat();

        [DataProperty, DataManualAuthoring(typeof(Int2Data))]
        public readonly Int2 Pivot => Get_Pivot();
    }

    public enum EnemyType : byte
    {
        [Description("N")] Normal = 0,
        [Description("E")] Elite = 1,
        [Description("B")] Boss = 2,
    }

    [Data]
    public partial struct Int2Data
    {
        [DataProperty] public readonly int X => Get_X();

        [DataProperty] public readonly int Y => Get_Y();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Int2(Int2Data data)
            => new() { x = data.X, y = data.Y };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Int2Data(Int2 value)
            => new() { _x = value.x, _y = value.y };
    }

    [Serializable]
    public struct Int2
    {
        public int x;
        public int y;
    }
}
