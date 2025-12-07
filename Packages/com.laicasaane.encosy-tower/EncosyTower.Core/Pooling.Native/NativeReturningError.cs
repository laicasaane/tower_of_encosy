using EncosyTower.EnumExtensions;

namespace EncosyTower.Pooling.Native
{
    [EnumExtensions]
    public enum NativeReturningError : byte
    {
        None = 0,
        ArraysMustHaveSameLength,
        ArraysMustContainAnyElement,
        NoPrefabAssociatedWithProvidedKey,
    }

    static partial class NativeReturningErrorExtensions { }
}
