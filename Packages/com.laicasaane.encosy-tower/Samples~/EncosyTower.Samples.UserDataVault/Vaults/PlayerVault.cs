using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.ConfigKeys;
using EncosyTower.Logging;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.UserDataVaults;

namespace EncosyTower.Samples.UserDataVault.Vaults;

#if UNITASK
using UnityTask = Cysharp.Threading.Tasks.UniTask;
using UnityTaskVault = Cysharp.Threading.Tasks.UniTask<PlayerVault.ReadOnlyVault>;
#else
using UnityTask = UnityEngine.Awaitable;
using UnityTaskVault = UnityEngine.Awaitable<PlayerVault.ReadOnlyVault>;
#endif

[UserDataVault]
public static partial class PlayerVault
{
    private static Vault s_vault;

    public static bool IsInitialized
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_vault != null;
    }

    public static ReadOnlyAccessorCollection Accessors
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_vault.Accessors;
    }

    public static async UnityTaskVault InitializeAsync(bool newGame, CancellationToken token)
    {
        var userId = API.GetOrGeneratePlayerId(forceGenerate: newGame);

        s_vault = new Vault(
              StringVault.Default
            , new NonEncryption(Logging.Logger.Default)
            , Logging.Logger.Default
            , ArrayPool<UnityTask>.Shared
            , userId
        );

        await s_vault.TryLoadAsync(
              Logging.Logger.Default
            , userId
            , SourcePriority.OnlyDevice
            , SaveDestination.Device
            , token
        ).SuppressCancellationThrow();

        return s_vault.AsReadOnly();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Save(CancellationToken token)
        => SaveAsync(false, token).Forget();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Save(bool verbose, CancellationToken token)
        => SaveAsync(verbose, token).Forget();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnityTask SaveAsync(CancellationToken token)
        => SaveAsync(false, token);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnityTask SaveAsync(bool verbose, CancellationToken token)
        => s_vault.SaveAsync(verbose, token);

    public static class API
    {
        private static readonly ConfigKey<string> s_key = "CURRENT_PLAYER_ID";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCurrentPlayerId()
            => s_key.GetPlayerPref().GetValueOrDefault(string.Empty).IsNotEmpty();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<string> TryGetCurrentPlayerId()
            => s_key.GetPlayerPref();

        public static string GetOrGeneratePlayerId(bool forceGenerate = false)
        {
            var valueOpt = s_key.GetPlayerPref();

            if (forceGenerate || valueOpt.TryGetValue(out var value) == false || value.IsEmpty())
            {
                value = GenerateId();
                s_key.SetPlayerPref(value);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DoesSaveFileExist()
        {
            return TryGetCurrentPlayerId().TryGetValue(out var value)
                && VaultAPI.SaveFileExists(value, nameof(StringIdCollection.PlayerData));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GenerateId()
        {
            var offset = new DateTimeOffset(DateTime.UtcNow.ToUniversalTime());
            return offset.ToUnixTimeMilliseconds().ToString();
        }
    }

    partial class DataDirectory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static partial void GetIgnoreEncryption(ref bool ignoreEncryption)
            => ignoreEncryption = true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static partial UserDataStoreArgs GetStoreArgs<TData, TStore>(Func<TData> createFunc)
            where TData : IUserData
            where TStore : UserDataStoreBase<TData>
            => VaultAPI.GetStoreArgs<TData, TStore>(createFunc);
    }

    partial class Vault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UnityTask SaveAsync(bool verbose, CancellationToken token)
        {
            await SaveAsync(SaveDestination.Device, token)
                .SuppressCancellationThrow();

            if (verbose)
            {
                StaticLogger.LogInfo($"Player data saved: {PlayerVault.API.TryGetCurrentPlayerId().GetValueOrDefault()}");
            }
        }
    }
}
