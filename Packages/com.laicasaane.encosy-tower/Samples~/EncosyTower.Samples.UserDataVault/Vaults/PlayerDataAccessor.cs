using System.ComponentModel;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using EncosyTower.Data;
using EncosyTower.Samples.UserDataVault.Shared;
using EncosyTower.Serialization.NewtonsoftJson.Collections;
using EncosyTower.UserDataVaults;
using Newtonsoft.Json;
using UnityEngine;

namespace EncosyTower.Samples.UserDataVault.Vaults;

[Data, DataMutable, DataWithoutId]
internal partial class PlayerData : IUserData
{
    [SerializeField, JsonProperty]
    [property: JsonIgnore]
    internal string _id;

    [SerializeField, JsonProperty]
    [property: JsonIgnore]
    internal string _version;

    [SerializeField, JsonProperty]
    [property: JsonIgnore]
    internal JsonArrayMap<ItemId, int> _itemAmounts = new();
}

[DisplayName("Player")]
[UserDataAccessor(typeof(PlayerVault))]
public sealed class PlayerDataAccessor : IUserDataAccessor
{
    private readonly UserDataStoreDefault<PlayerData> _store;

    internal PlayerDataAccessor(UserDataStoreDefault<PlayerData> store)
    {
        _store = store;
    }

    private PlayerData Data
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _store.Data;
    }

    public string Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Data._id;
    }

    public JsonArrayMap<ItemId, int>.ReadOnly ItemAmounts
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Data._itemAmounts;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MarkDirty()
    {
        _store.MarkDirty();
    }

    public Result<OnItemAmountUpdatedMsg, PlayerDataError> AddSingletonItem(ItemId id)
    {
        var error = PlayerDataError.ItemAlreadyAcquired(id).Prefix(nameof(AddSingletonItem));
        var result = Add(Data._itemAmounts, id, true, error);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault();
    }

    public Result<OnItemAmountUpdatedMsg, PlayerDataError> RemoveSingletonItem(ItemId id)
    {
        var error = PlayerDataError.ItemNotYetAcquired(id).Prefix(nameof(RemoveSingletonItem));
        var result = Remove(Data._itemAmounts, id, true, error);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsItemEnough(ItemId id, int requiredAmount)
    {
        return Data._itemAmounts.TryGetValue(id, out var currentAmount)
            && currentAmount >= requiredAmount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<int, PlayerDataError> GetItemAmount(ItemId id)
    {
        return Data._itemAmounts.TryGetValue(id, out var amount)
            ? amount
            : PlayerDataError.ItemNotYetAcquired(id).Prefix(nameof(GetItemAmount));
    }

    public Result<OnItemAmountUpdatedMsg, PlayerDataError> SetItemAmount(ItemId id, int amount)
    {
        var result = SetAmount(Data._itemAmounts, id, amount);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault().Prefix(nameof(SetItemAmount));
    }

    public Result<OnItemAmountUpdatedMsg, PlayerDataError> AddItemAmount(ItemId id, int amount)
    {
        var result = AddAmount(Data._itemAmounts, id, amount);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault().Prefix(nameof(AddItemAmount));
    }

    public Result<OnItemAmountUpdatedMsg, PlayerDataError> ReduceItemAmount(ItemId id, int amount)
    {
        var result = ReduceAmount(Data._itemAmounts, id, amount, PlayerDataError.ItemNotYetAcquired(id));
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault().Prefix(nameof(ReduceItemAmount));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsItemAcquired(ItemId itemId)
    {
        return Data._itemAmounts.ContainsKey(itemId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MarkDirtyIfSuccess<T>(Result<T, PlayerDataError> result)
    {
        if (result.IsSuccess)
        {
            MarkDirty();
        }
    }

    private static Result<Changed<int>, PlayerDataError> Add(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , bool singleton
        , PlayerDataError errorAlreadyAcquired
    )
    {
        if (singleton)
        {
            if (amountMap.ContainsKey(id))
            {
                return errorAlreadyAcquired;
            }

            amountMap[id] = 1;

            return new Changed<int>(1, 0);
        }

        return AddAmount(amountMap, id, 1);
    }

    private static Result<Changed<int>, PlayerDataError> Remove(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , bool singleton
        , PlayerDataError errorNotAcquired
    )
    {
        return singleton
            ? amountMap.Remove(id) ? new Changed<int>(0, 1) : errorNotAcquired
            : ReduceAmount(amountMap, id, 1, errorNotAcquired);
    }

    private static Result<Changed<int>, PlayerDataError> SetAmount(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , int amount
    )
    {
        if (amount < 1)
        {
            return PlayerDataError.AmountNegativeOrZero(amount, id);
        }

        ref var amountRef = ref amountMap.GetOrAdd(id);
        var previousAmount = amountRef;
        amountRef = amount;

        return new Changed<int>(amountRef, previousAmount);
    }

    private static Result<Changed<int>, PlayerDataError> AddAmount(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , int amount
    )
    {
        if (amount < 1)
        {
            return PlayerDataError.AmountNegativeOrZero(amount, id);
        }

        ref var amountRef = ref amountMap.GetOrAdd(id);
        var previousAmount = amountRef;
        amountRef += amount;

        return new Changed<int>(amountRef, previousAmount);
    }

    private static Result<Changed<int>, PlayerDataError> ReduceAmount(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , int amount
        , PlayerDataError errorNotAcquired
    )
    {
        if (amount < 1)
        {
            return PlayerDataError.AmountNegativeOrZero(amount, id);
        }

        if (amountMap.TryFindIndex(id, out var index) == false)
        {
            return errorNotAcquired;
        }

        ref var amountRef = ref amountMap.GetValueAtUnsafe(index);
        var previousAmount = amountRef;
        var newAmount = Mathf.Max(0, amountRef - amount);

        if (newAmount > 0)
        {
            amountRef = newAmount;
        }
        else
        {
            amountMap.Remove(id);
        }

        return new Changed<int>(newAmount, previousAmount);
    }
}
