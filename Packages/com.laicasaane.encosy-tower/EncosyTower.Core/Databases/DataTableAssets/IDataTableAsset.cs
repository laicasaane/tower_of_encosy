#pragma warning disable CA1040 // Avoid empty interfaces

using System;
using EncosyTower.Data;

namespace EncosyTower.Databases
{
    public interface IDataTableAsset { }

    public interface IDataTableAsset<TData>
        where TData : IData
    {
        ReadOnlyMemory<TData> Entries { get; }
    }
}
