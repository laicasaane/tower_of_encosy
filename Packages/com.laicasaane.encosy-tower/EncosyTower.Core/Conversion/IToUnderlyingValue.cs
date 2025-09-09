namespace EncosyTower.Conversion
{
    public interface IToUnderlyingValue<out T> where T : unmanaged
    {
        T ToUnderlyingValue();
    }
}
