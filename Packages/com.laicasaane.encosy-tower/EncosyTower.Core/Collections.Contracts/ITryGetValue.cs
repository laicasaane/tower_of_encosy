namespace EncosyTower.Collections
{
    public interface ITryGetValue<in TArg, TResult>
    {
        bool TryGetValue(TArg arg, out TResult result);
    }
}
