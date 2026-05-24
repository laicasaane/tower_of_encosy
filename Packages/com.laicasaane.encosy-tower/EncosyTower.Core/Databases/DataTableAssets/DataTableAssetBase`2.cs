using System.Runtime.CompilerServices;
using EncosyTower.Data;

namespace EncosyTower.Databases
{
    public abstract class DataTableAssetBase<TDataId, TData> : DataTableAssetBase<TDataId, TData, TDataId>, IDataTableAsset<TData>
        where TData : IData, IDataWithId<TDataId>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override TDataId ConvertId(TDataId value)
            => value;
    }
}
