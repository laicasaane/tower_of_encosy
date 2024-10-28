using EncosyTower.Modules.UnionIds;

namespace Module.GameCommon
{
    /// <summary>Resource ID</summary>
    [UnionId(Size = UnionIdSize.ULong, KindSettings = UnionIdKindSettings.PreserveOrder)]
    public readonly partial struct ResId { }

    [KindForUnionId(typeof(ResId), 100)]
    public enum HeroId : ushort { }

    [KindForUnionId(typeof(ResId), 200)]
    public enum EnemyId : ushort { }

    [KindForUnionId(typeof(ResId), 300)]
    public enum ItemId : ushort { }
}