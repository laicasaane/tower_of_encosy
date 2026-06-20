using EncosyTower.Common;

namespace EncosyTower.Loaders
{
    public interface ILoadOrError<TResult, TError>
    {
        Result<TResult, TError> LoadOrError();
    }
}
