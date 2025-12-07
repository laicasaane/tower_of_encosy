namespace EncosyTower.Collections
{
    public interface IHasCapacity
    {
        int Capacity { get; }
    }

    public interface IIncreaseCapacity : IHasCapacity
    {
        void IncreaseCapacityBy(int amount);

        void IncreaseCapacityTo(int newCapacity);
    }
}
