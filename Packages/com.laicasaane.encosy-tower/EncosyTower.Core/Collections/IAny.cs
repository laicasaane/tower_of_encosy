namespace EncosyTower.Collections
{
    public interface IAny<in T>
    {
        bool Any(T value);
    }
}
