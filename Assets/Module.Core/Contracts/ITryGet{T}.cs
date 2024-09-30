namespace Module.Core
{
    public interface ITryGet<T>
    {
        bool TryGet(out T result);
    }
}
