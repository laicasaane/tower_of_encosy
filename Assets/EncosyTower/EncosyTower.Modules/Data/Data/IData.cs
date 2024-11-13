#pragma warning disable CA1040 // Avoid empty interfaces

namespace EncosyTower.Modules.Data
{
    public interface IData { }

    public interface IDataWithId<out TDataId> : IData
    {
        TDataId Id { get; }
    }
}
