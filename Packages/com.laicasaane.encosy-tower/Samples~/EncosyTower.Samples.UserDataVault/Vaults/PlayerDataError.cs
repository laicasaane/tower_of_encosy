using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Samples.UserDataVault.Shared;
using Unity.Collections;

namespace EncosyTower.Samples.UserDataVault.Vaults;

public readonly struct PlayerDataError
{
    private record struct Data(
          FixedString32Bytes Prefix
        , Option<ItemId> ItemId
        , Option<int> Int
    );

    private enum Type : byte
    {
        Unknown = 0,
        ItemNotYetAcquired,
        ItemAlreadyAcquired,
        AmountNegativeOrZero,
    }

    private readonly Type _type;
    private readonly Data _data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PlayerDataError(Type type, Data data)
    {
        _type = type;
        _data = data;
    }

    public static PlayerDataError ItemNotYetAcquired(ItemId id)
        => new(Type.ItemNotYetAcquired, new Data() with { ItemId = id });

    public static PlayerDataError ItemAlreadyAcquired(ItemId id)
        => new(Type.ItemAlreadyAcquired, new Data() with { ItemId = id });

    public static PlayerDataError AmountNegativeOrZero(int amount, Option<ItemId> id = default)
        => new(Type.AmountNegativeOrZero, new Data() with { ItemId = id, Int = amount });

    public PlayerDataError Prefix(FixedString32Bytes prefix)
        => new(_type, _data with { Prefix = prefix });

    public string ToMessage()
    {
        return _type switch
        {
            Type.ItemNotYetAcquired => ToMessage_ItemNotYetAcquired(),
            Type.ItemAlreadyAcquired => ToMessage_ItemAlreadyAcquired(),
            Type.AmountNegativeOrZero => ToMessage_AmountNegativeOrZero(),
            _ => "An unknown error has occurred.",
        };
    }

    private string ToMessage_ItemNotYetAcquired()
    {
        var fs = InitFixedString(_data);

        fs.Append("The item '");
        fs.Append(_data.ItemId.GetValueOrThrow().ToFixedString());
        fs.Append("' has not been acquired yet.");

        return fs.ToString();
    }

    private string ToMessage_ItemAlreadyAcquired()
    {
        var fs = InitFixedString(_data);

        fs.Append("The item '");
        fs.Append(_data.ItemId.GetValueOrThrow().ToFixedString());
        fs.Append("' has already been acquired.");

        return fs.ToString();
    }

    private string ToMessage_AmountNegativeOrZero()
    {
        var fs = InitFixedString(_data);
        var amount = _data.Int.GetValueOrThrow();

        if (_data.ItemId.TryGetValue(out var id))
        {
            fs.Append("The amount for item '");
            fs.Append(id.ToFixedString());
            fs.Append("' must be positive, but it is '");
            fs.Append(amount);
            fs.Append("'.");
        }
        else
        {
            fs.Append("The amount must be positive, but it is '");
            fs.Append(amount);
            fs.Append("'.");
        }

        return fs.ToString();
    }

    private static FixedString512Bytes InitFixedString(in Data data)
    {
        FixedString512Bytes fs = default;

        if (data.Prefix.IsEmpty == false)
        {
            fs.Append('[');
            fs.Append(data.Prefix);
            fs.Append(']');
            fs.Append(' ');
        }

        return fs;
    }
}
