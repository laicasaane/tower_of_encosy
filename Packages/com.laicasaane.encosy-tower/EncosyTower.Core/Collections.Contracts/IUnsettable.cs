namespace EncosyTower.Collections
{
    public interface IUnsettable<T>
    {
        T Unset(T value);
    }
}
