namespace EncosyTower.Buffers
{
    public interface IAlloc
    {
        void Alloc(int size, AllocatorStrategy allocatorStrategy, bool memClear = true);
    }
}
