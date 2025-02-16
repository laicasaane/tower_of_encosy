using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Data;
using EncosyTower.TypeWraps;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.Data
{
    [WrapRecord]
    public readonly partial record struct HeroId(short Value);

    [WrapRecord]
    public readonly partial record struct EnemyId(short Value);

    [UnionId(KindSettings = UnionIdKindSettings.PreserveOrder | UnionIdKindSettings.RemoveSuffix)]
    [UnionIdKind(typeof(HeroId), 0, signed: true)]
    [UnionIdKind(typeof(EnemyId), 1, signed: true)]
    public readonly partial struct EntityId
    {
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct EntityUid : IEquatable<EntityUid>
    {
        [FieldOffset(0)] private readonly ulong _raw;

        [FieldOffset(0)] public readonly byte Rarity;
        [FieldOffset(4)] public readonly EntityId Id;

        public EntityUid(EntityId id, byte rarity) : this()
        {
            Id = id;
            Rarity = rarity;
        }

        public static bool operator ==(EntityUid a, EntityUid b)
            => a._raw == b._raw;

        public static bool operator !=(EntityUid a, EntityUid b)
            => a._raw != b._raw;

        public bool Equals(EntityUid other)
            => _raw == other._raw;

        public override bool Equals(object obj)
            => obj is EntityUid other && Equals(other);

        public override int GetHashCode()
            => _raw.GetHashCode();
    }

    [DataWithoutId]
    public partial struct EntityIdData : IData
    {
        [DataProperty] public readonly string Kind => Get_Kind();

        [DataProperty] public readonly short SubId => Get_SubId();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator EntityId(EntityIdData data)
        {
            var @default = default(EntityId);
            var result = @default.TryParse(data._kind, data._subId, out var value, false, true);

            return result ? value : @default;
        }
    }

    [DataWithoutId]
    public partial struct EntityUidData : IData
    {
        [DataProperty] public readonly EntityIdData EntityId => Get_EntityId();

        [DataProperty] public readonly byte Rarity => Get_Rarity();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator EntityUid(EntityUidData data)
            => new((EntityId)data._entityId, data._rarity);
    }

    public partial struct EntityStatData : IData
    {
        [DataProperty] public readonly float Hp => Get_Hp();

        [DataProperty] public readonly float Atk => Get_Atk();
    }

    public partial struct EntityStatMultiplierData : IData
    {
        [DataProperty] public readonly int Level => Get_Level();

        [DataProperty] EntityStatData Multiplier => Get_Multiplier();
    }
}
