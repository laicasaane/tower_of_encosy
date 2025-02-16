namespace EncosyTower.Loaders
{
    public interface ILoad<out T>
    {
        T Load();
    }
}
