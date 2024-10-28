namespace EncosyTower.Modules.Buffers
{
    public interface IBufferProvider<T>
    {
        ref T[] Buffer { get; }

        ref int Count { get; }

        ref int Version { get; }
    }
}
