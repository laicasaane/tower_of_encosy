namespace EncosyTower.Modules
{
    public interface ITryLoad<T>
    {
        Option<T> TryLoad();
    }
}
