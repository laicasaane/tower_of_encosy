using EncosyTower.Data;
using EncosyTower.Data.Authoring;
using EncosyTower.Databases;
using EncosyTower.StringIds;

namespace EncosyTower.Samples.Data
{
    [DataTableAsset]
    public sealed partial class StringTableAsset : DataTableAssetBase<StringId, StringData>
    {
    }

    [Data]
    public partial struct StringData
    {
        [DataProperty(typeof(uint), typeof(StringIdValueConverter))]
        public readonly StringId Id => Get_Id();

        [DataProperty, DataManualAuthoring(typeof(string))]
        public readonly string Value => Get_Value();
    }
}
