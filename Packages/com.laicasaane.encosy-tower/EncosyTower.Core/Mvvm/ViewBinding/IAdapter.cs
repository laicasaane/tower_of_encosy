using EncosyTower.Variants;

namespace EncosyTower.Mvvm.ViewBinding
{
    public interface IAdapter
    {
        Variant Convert(in Variant variant);
    }
}
