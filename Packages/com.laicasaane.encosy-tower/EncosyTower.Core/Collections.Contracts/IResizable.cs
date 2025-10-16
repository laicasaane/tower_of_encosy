namespace EncosyTower.Collections
{
    public interface IResizable
    {
        void Resize(int newCapacity);

        void Resize(int newCapacity, bool copyContent);
    }
}
