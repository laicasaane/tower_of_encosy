namespace EncosyTower.Collections
{
    public interface ITryGet<T>
    {
        bool TryGet(out T result);
    }
}
