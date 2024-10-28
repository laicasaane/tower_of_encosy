using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.ViewBinding
{
    public interface IAdapter
    {
        Union Convert(in Union union);
    }
}
