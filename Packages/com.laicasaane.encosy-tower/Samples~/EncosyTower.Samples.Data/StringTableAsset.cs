using System.Runtime.CompilerServices;
using EncosyTower.Data;
using EncosyTower.Data.Authoring;
using EncosyTower.Databases;
using EncosyTower.StringIds;

namespace EncosyTower.Samples.Data
{
    [DataTableAsset]
    public sealed partial class StringTableAsset : DataTableAssetBase<uint, StringData, StringId>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override StringId ConvertId(uint value)
            => UIntToStringIdConverter.Convert(value);
    }

    [Data]
    public partial struct StringData
    {
        [DataProperty]
        public readonly uint Id => Get_Id();

        [DataProperty, DataManualAuthoring(typeof(string))]
        public readonly string Value => Get_Value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringId Convert(uint value)
            => UIntToStringIdConverter.Convert(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Convert(StringId value)
            => StringIdToUIntConverter.Convert(value);
    }
}
