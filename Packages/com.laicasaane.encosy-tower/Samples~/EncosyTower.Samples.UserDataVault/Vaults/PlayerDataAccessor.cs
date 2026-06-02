using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Common;
using EncosyTower.Samples.UserDataVault.Shared;
using EncosyTower.Serialization.Collections;
using EncosyTower.UserDataVaults;
using UnityEngine;

namespace EncosyTower.Samples.UserDataVault.Vaults;

using Error = PlayerDataError;

[Serializable, UserData]
internal partial class PlayerData
{
    public JsonArrayMap<ItemId, int> ItemAmounts { get; set; } = new();
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
        get => Data.Id;
    }

    public JsonArrayMap<ItemId, int>.ReadOnly ItemAmounts
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Data.ItemAmounts;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MarkDirty()
    {
        _store.MarkDirty();
    }

    public Result<OnItemAmountUpdatedMsg, Error> AddSingletonItem(ItemId id)
    {
        var error = Error.ItemAlreadyAcquired(id).Prefix(nameof(AddSingletonItem));
        var result = Add(Data.ItemAmounts, id, true, error);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault();
    }

    public Result<OnItemAmountUpdatedMsg, Error> RemoveSingletonItem(ItemId id)
    {
        var error = Error.ItemNotYetAcquired(id).Prefix(nameof(RemoveSingletonItem));
        var result = Remove(Data.ItemAmounts, id, true, error);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsItemEnough(ItemId id, int requiredAmount)
    {
        return Data.ItemAmounts.TryGetValue(id, out var currentAmount)
            && currentAmount >= requiredAmount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<int, Error> GetItemAmount(ItemId id)
    {
        return Data.ItemAmounts.TryGetValue(id, out var amount)
            ? amount
            : Error.ItemNotYetAcquired(id).Prefix(nameof(GetItemAmount));
    }

    public Result<OnItemAmountUpdatedMsg, Error> SetItemAmount(ItemId id, int amount)
    {
        var result = SetAmount(Data.ItemAmounts, id, amount);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault().Prefix(nameof(SetItemAmount));
    }

    public Result<OnItemAmountUpdatedMsg, Error> AddItemAmount(ItemId id, int amount)
    {
        var result = AddAmount(Data.ItemAmounts, id, amount);
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault().Prefix(nameof(AddItemAmount));
    }

    public Result<OnItemAmountUpdatedMsg, Error> ReduceItemAmount(ItemId id, int amount)
    {
        var result = ReduceAmount(Data.ItemAmounts, id, amount, Error.ItemNotYetAcquired(id));
        MarkDirtyIfSuccess(result);

        return result.TryGetValue(out var changed)
            ? new OnItemAmountUpdatedMsg(id, changed)
            : result.GetErrorOrDefault().Prefix(nameof(ReduceItemAmount));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsItemAcquired(ItemId itemId)
    {
        return Data.ItemAmounts.ContainsKey(itemId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MarkDirtyIfSuccess<T>(Result<T, Error> result)
    {
        if (result.IsSuccess)
        {
            MarkDirty();
        }
    }

    private static Result<Changed<int>, Error> Add(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , bool singleton
        , Error errorAlreadyAcquired
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

    private static Result<Changed<int>, Error> Remove(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , bool singleton
        , Error errorNotAcquired
    )
    {
        return singleton
            ? amountMap.Remove(id) ? new Changed<int>(0, 1) : errorNotAcquired
            : ReduceAmount(amountMap, id, 1, errorNotAcquired);
    }

    private static Result<Changed<int>, Error> SetAmount(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , int amount
    )
    {
        if (amount < 1)
        {
            return Error.ItemAmountNegativeOrZero(id, amount);
        }

        ref var amountRef = ref amountMap.GetOrAdd(id);
        var previousAmount = amountRef;
        amountRef = amount;

        return new Changed<int>(amountRef, previousAmount);
    }

    private static Result<Changed<int>, Error> AddAmount(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , int amount
    )
    {
        if (amount < 1)
        {
            return Error.ItemAmountNegativeOrZero(id, amount);
        }

        ref var amountRef = ref amountMap.GetOrAdd(id);
        var previousAmount = amountRef;
        amountRef += amount;

        return new Changed<int>(amountRef, previousAmount);
    }

    private static Result<Changed<int>, Error> ReduceAmount(
          JsonArrayMap<ItemId, int> amountMap
        , ItemId id
        , int amount
        , Error errorNotAcquired
    )
    {
        if (amount < 1)
        {
            return Error.ItemAmountNegativeOrZero(id, amount);
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
