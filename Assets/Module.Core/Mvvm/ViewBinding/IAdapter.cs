using Module.Core.Unions;

namespace Module.Core.Mvvm.ViewBinding
{
    public interface IAdapter
    {
        Union Convert(in Union union);
    }
}
