namespace EncosyTower.Conversion
{
    public interface ITryConvert<T>
    {
        bool TryConvert(out T result);
    }
}
