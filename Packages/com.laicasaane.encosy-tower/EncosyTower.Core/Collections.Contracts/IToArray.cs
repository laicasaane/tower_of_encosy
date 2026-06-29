namespace EncosyTower.Collections
{
    public interface IToArray<out T>
    {
        T[] ToArray();
    }
}
