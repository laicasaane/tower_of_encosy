using EncosyTower.Unions;

namespace EncosyTower.Mvvm.ViewBinding
{
    public interface IAdapter
    {
        Union Convert(in Union union);
    }
}
