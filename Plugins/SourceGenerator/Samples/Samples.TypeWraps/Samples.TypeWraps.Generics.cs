using EncosyTower.TypeWraps;

namespace Samples.TypeWraps.Generics
{
    [WrapType(typeof(int), "value")]
    public partial struct Index<T> where T : unmanaged
    {
        public int value;
    }
}
