namespace EncosyTower.Modules
{
    public interface IToUnderlyingValue<T> where T : unmanaged
    {
        T ToUnderlyingValue();
    }
}
