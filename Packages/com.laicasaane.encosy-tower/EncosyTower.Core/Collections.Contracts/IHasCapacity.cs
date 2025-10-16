namespace EncosyTower.Collections
{
    public interface IHasCapacity
    {
        int Capacity { get; }

        void IncreaseCapacityBy(int amount);

        void IncreaseCapacityTo(int newCapacity);
    }
}
