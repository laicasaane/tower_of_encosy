namespace EncosyTower.Modules
{
    public interface ITryConvert<T>
    {
        bool TryConvert(out T result);
    }
}
