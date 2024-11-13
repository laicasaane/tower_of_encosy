namespace EncosyTower.Modules
{
    public interface ILoad<out T>
    {
        T Load();
    }
}
