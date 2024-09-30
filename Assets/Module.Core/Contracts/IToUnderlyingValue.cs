namespace Module.Core
{
    public interface IToUnderlyingValue<T> where T : unmanaged
    {
        T ToUnderlyingValue();
    }
}
