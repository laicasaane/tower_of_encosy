using System.Runtime.CompilerServices;
using EncosyTower.Common;
using Unity.Entities;

namespace EncosyTower.Entities.Stats
{
    public struct StatReader<TValuePair, TStat> : IIsCreated
        where TValuePair : unmanaged, IStatValuePair
        where TStat : unmanaged, IStat<TValuePair>
    {
        private BufferLookup<TStat> _lookupStats;
        private DynamicBuffer<TStat> _statBuffer;
        private readonly ByteBool _isCreated;
        private readonly ByteBool _useLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal StatReader(BufferLookup<TStat> lookupStats) : this()
        {
            _lookupStats = lookupStats;
            _isCreated = true;
            _useLookup = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal StatReader(DynamicBuffer<TStat> statBuffer) : this()
        {
            _statBuffer = statBuffer;
            _isCreated = true;
            _useLookup = false;
        }

        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isCreated;
        }

        public readonly bool UseLookup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _useLookup;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(StatHandle statHandle)
        {
            return UseLookup
                ? StatAPI.Contains<TValuePair, TStat>(statHandle, _lookupStats)
                : StatAPI.Contains<TValuePair, TStat>(statHandle, _statBuffer.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(StatHandle statHandle, uint userData)
        {
            return UseLookup
                ? StatAPI.Contains<TValuePair, TStat>(statHandle, userData, _lookupStats)
                : StatAPI.Contains<TValuePair, TStat>(statHandle, userData, _statBuffer.AsNativeArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStatData<TStatData>(StatHandle<TStatData> statHandle, out TStatData statData)
            where TStatData : unmanaged, IStatData
        {
            return UseLookup
                ? StatAPI.TryGetStatData<TValuePair, TStat, TStatData>(statHandle, _lookupStats, out statData)
                : StatAPI.TryGetStatData<TValuePair, TStat, TStatData>(statHandle, _statBuffer.AsNativeArray(), out statData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStat(StatHandle statHandle, out TStat stat)
        {
            return UseLookup
                ? StatAPI.TryGetStat<TValuePair, TStat>(statHandle, _lookupStats, out stat)
                : StatAPI.TryGetStat<TValuePair, TStat>(statHandle, _statBuffer.AsNativeArray(), out stat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetStatValue(StatHandle statHandle, out TValuePair valuePair)
        {
            return UseLookup
                ? StatAPI.TryGetStatValue<TValuePair, TStat>(statHandle, _lookupStats, out valuePair)
                : StatAPI.TryGetStatValue<TValuePair, TStat>(statHandle, _statBuffer.AsNativeArray(), out valuePair);
        }
    }
}
