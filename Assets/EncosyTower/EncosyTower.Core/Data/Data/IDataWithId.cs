namespace EncosyTower.Data
{
    public interface IDataWithId<out TDataId> : IData
    {
        TDataId Id { get; }
    }
}
