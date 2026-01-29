using System;
using EncosyTower.Conversion;
using EncosyTower.TypeWraps;
using EncosyTower.UnionIds;
using Unity.Collections;

namespace EncosyTower.Tests.UnionIds
{
    [UnionId(Size = UnionIdSize.ULong2)]
    [UnionIdKind(typeof(MusicType), 0, "Music")]
    [UnionIdKind(typeof(SpecialId), 4)]
    [UnionIdKind(typeof(ComplexId), 5)]
    public readonly partial struct AudioId
    {
        private static partial bool TryParse_ComplexId(
              ReadOnlySpan<char> str
            , out ComplexId value
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            throw new NotImplementedException();
        }

        private static partial bool TryParse_SpecialId(
              ReadOnlySpan<char> str
            , out SpecialId value
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            throw new NotImplementedException();
        }

        private static partial void Append_ComplexId(ref FixedString64Bytes fs, ComplexId value, bool isDisplay)
        {
            throw new NotImplementedException();
        }
    }

    [EncosyTower.EnumExtensions.EnumExtensions]
    public enum MusicType : ushort
    {
        None = 0,
        Main = 1,
        Combat = 2,
    }

    partial class MusicTypeExtensions { }

    [KindForUnionId(typeof(AudioId), 1, displayName: "Common")]
    public enum SoundCommon : byte
    {
        None = 0,
        Click = 1,
    }

    [KindForUnionId(typeof(AudioId), 2, displayName: "Gameplay")]
    public enum SoundGameplay : byte
    {
        None = 0,
        Attack = 1,
        Hit = 2,
        Die = 3,
    }

    [WrapRecord]
    public readonly partial record struct SpecialId(uint Id)
        : IToFixedString<FixedString32Bytes>
        , IToDisplayFixedString<FixedString32Bytes>
    {
        public FixedString32Bytes ToDisplayFixedString()
        {
            throw new NotImplementedException();
        }

        public FixedString32Bytes ToFixedString()
        {
            throw new NotImplementedException();
        }
    }

    public readonly struct ComplexId : IToFixedString<FixedString32Bytes>
    {
        public readonly byte Id1;
        public readonly byte Id2;

        public FixedString32Bytes ToFixedString()
        {
            throw new NotImplementedException();
        }
    }

    static partial class ResourceIdEnumeration
    {
        public static void Do()
        {
            //Length
        }
    }

    [UnionId]
    [UnionIdKind(typeof(MoneyType), 0, "Money", signed: false)]
    [UnionIdKind(typeof(EquipmentId), 1, "Equipment", signed: false)]
    public readonly partial struct ResourceId
    {
    }

    public enum MoneyType : ushort
    {
        Gold = 0,
        Diamond = 1,
    }

    [WrapRecord]
    public readonly partial record struct OutfitId(byte Value)
        : ITryParseSpan<OutfitId>
        , IToDisplayString
        , IToFixedString<FixedString32Bytes>
        , IToDisplayFixedString<FixedString32Bytes>
    {
        public string ToDisplayString()
        {
            throw new NotImplementedException();
        }

        public FixedString32Bytes ToFixedString()
        {
            throw new NotImplementedException();
        }

        public FixedString32Bytes ToDisplayFixedString()
        {
            throw new NotImplementedException();
        }

        public bool TryParse(
              ReadOnlySpan<char> str
            , out OutfitId result
            , bool ignoreCase
            , bool allowMatchingMetadataAttribute
        )
        {
            throw new NotImplementedException();
        }
    }

    [UnionId]
    [UnionIdKind(typeof(OutfitId), 0, displayName: "Outfit", signed: false)]
    public readonly partial struct EquipmentId
    {
    }
}