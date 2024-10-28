namespace EncosyTower.Modules
{
    public interface ITryGet<T>
    {
        bool TryGet(out T result);
    }
}
