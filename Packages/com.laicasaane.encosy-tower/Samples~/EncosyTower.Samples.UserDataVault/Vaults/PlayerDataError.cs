using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.PolyEnumStructs;
using EncosyTower.Samples.UserDataVault.Shared;
using Unity.Collections;

namespace EncosyTower.Samples.UserDataVault.Vaults;

[PolyEnumFactoryFor(typeof(Error))]
public readonly partial struct PlayerDataError
{
    private readonly FixedString64Bytes _prefix;
    private readonly Error _error;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PlayerDataError(in Error error) : this(error, default)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PlayerDataError(in Error error, in FixedString64Bytes prefix)
    {
        _prefix = prefix;
        _error = error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PlayerDataError Prefix(in FixedString64Bytes prefix)
        => new(_error, prefix);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => _error.ToMessage(_prefix).ToString();

    [PolyEnumStruct]
    readonly partial struct Error
    {
        private static FixedString512Bytes InitFixedString(in FixedString64Bytes prefix)
        {
            FixedString512Bytes fs = default;

            if (prefix.IsEmpty == false)
            {
                fs.Append('[');
                fs.Append(prefix);
                fs.Append(']');
                fs.Append(' ');
            }

            return fs;
        }

        partial interface IEnumCase
        {
            FixedString512Bytes ToMessage(in FixedString64Bytes prefix);
        }

        public readonly partial struct Undefined
        {
            public FixedString512Bytes ToMessage(in FixedString64Bytes prefix)
            {
                FixedString64Bytes m = "An unknown error has occurred.";
                var fs = InitFixedString(prefix);
                fs.Append(m);
                return fs;
            }
        }

        public readonly partial record struct ItemNotYetAcquired(ItemId Id)
        {
            public FixedString512Bytes ToMessage(in FixedString64Bytes prefix)
            {
                FixedString64Bytes m1 = "The item '";
                FixedString64Bytes m2 = "' has not been acquired yet.";
                var fs = InitFixedString(prefix);
                fs.Append(m1);
                fs.Append(Id.ToFixedString());
                fs.Append(m2);
                return fs;
            }
        }

        public readonly partial record struct ItemAlreadyAcquired(ItemId Id)
        {
            public FixedString512Bytes ToMessage(in FixedString64Bytes prefix)
            {
                FixedString64Bytes m1 = "The item '";
                FixedString64Bytes m2 = "' has already been acquired.";
                var fs = InitFixedString(prefix);
                fs.Append(m1);
                fs.Append(Id.ToFixedString());
                fs.Append(m2);
                return fs;
            }
        }

        public readonly partial record struct ItemAmountNegativeOrZero(ItemId Id, int Amount)
        {
            public FixedString512Bytes ToMessage(in FixedString64Bytes prefix)
            {
                FixedString64Bytes m1 = "The amount for item '";
                FixedString64Bytes m2 = "' must be positive, but it is '";
                FixedString64Bytes m3 = "'.";
                var fs = InitFixedString(prefix);
                fs.Append(m1);
                fs.Append(Id.ToFixedString());
                fs.Append(m2);
                fs.Append(Amount.ToString());
                fs.Append(m3);
                return fs;
            }
        }

        public readonly partial record struct AmountNegativeOrZero(int Amount)
        {
            public FixedString512Bytes ToMessage(in FixedString64Bytes prefix)
            {
                FixedString64Bytes m1 = "The amount must be positive, but it is '";
                FixedString64Bytes m2 = "'.";
                var fs = InitFixedString(prefix);
                fs.Append(m1);
                fs.Append(Amount.ToString());
                fs.Append(m2);
                return fs;
            }
        }
    }
}
