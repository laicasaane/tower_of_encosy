using System.Runtime.CompilerServices;

namespace EncosyTower.Entities.Stats
{
    public readonly struct StatData<TStatData> where TStatData : unmanaged, IStatData
    {
        public readonly TStatData Data;
        public readonly bool ProduceChangeEvents;
        public readonly bool IsValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatData(TStatData data, bool produceChangeEvents = false)
        {
            Data = data;
            ProduceChangeEvents = produceChangeEvents;
            IsValid = true;
        }
    }
}
