namespace EncosyTower.Modules
{
    public interface IToUnderlyingValue<out T> where T : unmanaged
    {
        T ToUnderlyingValue();
    }
}
