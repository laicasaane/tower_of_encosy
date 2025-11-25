using EncosyTower.Entities.Stats;

namespace EncosyTower.Tests.Entities.Stats
{
    [StatSystem(StatDataSize.Size8)]
    public partial struct StatSystem
    {
        public static void Test()
        {
        }
    }

    [StatData(StatVariantType.Float)]
    public partial struct Hp
    {
    }

    [StatData(typeof(DirectionType), true)]
    public partial struct Direction
    {
    }

    public enum DirectionType : byte
    {
        Forward, Backward, Up, Down, Left,
    }
}
