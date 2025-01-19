namespace EncosyTower.Modules.Data
{
    public interface IDataWithId<out TDataId> : IData
    {
        TDataId Id { get; }
    }
}
