namespace EncosyTower.Data
{
    public interface IDataWithReadOnlyView<out T>
    {
        T AsReadOnly();
    }

    public interface IReadOnlyData<out T> : IData where T : IData { }
}
