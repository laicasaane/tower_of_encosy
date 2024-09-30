namespace Module.Core
{
    public interface ITryConvert<T>
    {
        bool TryConvert(out T result);
    }
}
