namespace EncosyTower.Collections
{
    public interface ITrySet<in T>
    {
        bool TrySet(T result);
    }
}
