using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Entities.Stats
{
    public readonly struct StatData<TStatData> where TStatData : unmanaged, IStatData
    {
        public readonly TStatData Data;
        public readonly uint UserData;
        public readonly bool ProduceChangeEvents;
        public readonly bool IsValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatData(TStatData data, bool produceChangeEvents = false, uint userData = default)
        {
            Data = data;
            UserData = userData;
            ProduceChangeEvents = produceChangeEvents;
            IsValid = true;
        }
    }

    public readonly struct StatDataParams<TStatData> where TStatData : unmanaged, IStatData
    {
        public readonly Option<TStatData> StatData;
        public readonly Option<StatHandle<TStatData>> Handle;
        public readonly Option<uint> UserData;
        public readonly Option<bool> ProduceChangeEvents;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatDataParams(
              Option<TStatData> statData = default
            , Option<StatHandle<TStatData>> handle = default
            , Option<bool> produceChangeEvents = default
            , Option<uint> userData = default
        )
        {
            StatData = statData;
            Handle = handle;
            UserData = userData;
            ProduceChangeEvents = produceChangeEvents;
        }

        public bool IsCreated
            => StatData.HasValue || Handle.HasValue || UserData.HasValue || ProduceChangeEvents.HasValue;
    }

    public readonly struct StatValueParams<TValuePair> where TValuePair : unmanaged, IStatValuePair
    {
        public readonly Option<TValuePair> StatValues;
        public readonly Option<StatHandle> Handle;
        public readonly Option<uint> UserData;
        public readonly Option<bool> ProduceChangeEvents;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatValueParams(
              Option<TValuePair> statValues = default
            , Option<StatHandle> handle = default
            , Option<bool> produceChangeEvents = default
            , Option<uint> userData = default
        )
        {
            StatValues = statValues;
            Handle = handle;
            UserData = userData;
            ProduceChangeEvents = produceChangeEvents;
        }

        public bool IsCreated
            => StatValues.HasValue || Handle.HasValue || UserData.HasValue || ProduceChangeEvents.HasValue;
    }
}
