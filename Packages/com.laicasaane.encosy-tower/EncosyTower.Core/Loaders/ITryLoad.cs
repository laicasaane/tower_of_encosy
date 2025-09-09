using EncosyTower.Common;

namespace EncosyTower.Loaders
{
    public interface ITryLoad<T>
    {
        Option<T> TryLoad();
    }
}
