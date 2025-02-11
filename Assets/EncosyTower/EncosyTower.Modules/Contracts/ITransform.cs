namespace EncosyTower.Modules
{
    public interface ITransform<in TFrom, out TTo>
    {
        TTo Transform(TFrom from);
    }
}
