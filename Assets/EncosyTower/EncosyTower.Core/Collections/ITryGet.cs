namespace EncosyTower.Collections
{
    public interface ITryGet<T>
    {
        bool TryGet(out T result);
    }

    public interface ITryGet<in TArg, TResult>
    {
        bool TryGet(TArg arg, out TResult result);
    }
}
