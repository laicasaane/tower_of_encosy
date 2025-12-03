using EncosyTower.TypeWraps;

namespace EncosyTower.Tests.TypeWrapGenerics
{
    [WrapType(typeof(int), "value")]
    public partial struct Index<T> where T : unmanaged
    {
        public int value;
    }
}
