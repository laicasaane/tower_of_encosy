using EncosyTower.Common;
using EncosyTower.Logging;

namespace EncosyTower.Samples.UserDataVault.Vaults;

public static class PlayerDataResultExtensions
{
    public static bool AssertError(this Result<OnItemAmountUpdatedMsg, PlayerDataError> result, bool log = false)
    {
        if (result.TryGetError(out var error) == false)
        {
            return false;
        }

        if (log)
        {
            StaticLogger.LogWarning(error);
        }

        return true;
    }
}
