namespace Module.Core
{
    public interface ITryLoad<T>
    {
        Option<T> TryLoad();
    }
}
