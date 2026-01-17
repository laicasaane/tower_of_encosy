using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Entities.Stats
{
    public readonly struct StatData<TStatData> : IIsCreated
        where TStatData : unmanaged, IStatData
    {
        public readonly TStatData Data;
        public readonly uint UserData;
        public readonly bool ProduceChangeEvents;
        private readonly bool _isCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatData(TStatData data, bool produceChangeEvents = false, uint userData = default)
        {
            Data = data;
            UserData = userData;
            ProduceChangeEvents = produceChangeEvents;
            _isCreated = true;
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isCreated;
        }
    }

    public readonly struct StatDataParams<TStatData> : IIsCreated
        where TStatData : unmanaged, IStatData
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

    public readonly struct StatValueParams<TValuePair> : IIsCreated
        where TValuePair : unmanaged, IStatValuePair
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
