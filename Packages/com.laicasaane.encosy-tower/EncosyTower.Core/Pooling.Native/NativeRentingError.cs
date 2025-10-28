using EncosyTower.EnumExtensions;

namespace EncosyTower.Pooling.Native
{
    [EnumExtensions]
    public enum NativeRentingError : byte
    {
        None = 0,
        ArraysMustHaveSameLength,
        AmountMustBeGreaterThanZero,
        ResultListMustBeCreatedInAdvance,
    }
}
