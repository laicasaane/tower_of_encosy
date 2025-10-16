namespace EncosyTower.Collections
{
    public interface IContains<in T>
    {
        bool Contains(T value);
    }
}
