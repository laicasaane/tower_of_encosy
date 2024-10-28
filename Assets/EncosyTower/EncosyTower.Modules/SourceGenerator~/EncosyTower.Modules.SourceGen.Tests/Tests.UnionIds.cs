using EncosyTower.Modules.UnionIds;

namespace EncosyTower.Modules.Tests.UnionIds
{
    [UnionId]
    [UnionIdKind(typeof(MusicType), 0)]
    public readonly partial struct AudioId
    {
    }

    public enum MusicType : ushort
    {
        None = 0,
        Main = 1,
        Combat = 2,
    }

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

    [UnionId(KindSettings = UnionIdKindSettings.PreserveOrder)]
    public readonly partial struct ResourceId { }

    static partial class ResourceIdEnumeration
    {
        public static void Do()
        {
            //Length
        }
    }
}