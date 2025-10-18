using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace EncosyTower.Buffers
{
    internal enum AllocatorStrategyType : uint
    {
        None = 0,
        Allocator = 1,
        AllocatorHandle = 2,
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct AllocatorStrategy
    {
        [FieldOffset(0)] private readonly Allocator _allocator;
        [FieldOffset(4)] private readonly AllocatorStrategyType _type;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllocatorStrategy(Allocator allocator) : this()
        {
            _allocator = allocator;
            _type = AllocatorStrategyType.Allocator;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _type is (AllocatorStrategyType.Allocator or AllocatorStrategyType.AllocatorHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AllocatorStrategy(Allocator allocator)
            => new(allocator);

        public bool TryGetAllocator(out Allocator result)
        {
            if (_type == AllocatorStrategyType.Allocator)
            {
                result = _allocator;
                return true;
            }

            result = default;
            return false;
        }

#if UNITY_COLLECTIONS
        [FieldOffset(0)] private readonly AllocatorManager.AllocatorHandle _handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllocatorStrategy(AllocatorManager.AllocatorHandle handle) : this()
        {
            _handle = handle;
            _type = AllocatorStrategyType.AllocatorHandle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AllocatorStrategy(AllocatorManager.AllocatorHandle handle)
            => new(handle);

        public bool TryGetAllocatorHandle(out AllocatorManager.AllocatorHandle result)
        {
            if (_type == AllocatorStrategyType.AllocatorHandle)
            {
                result = _handle;
                return true;
            }

            result = default;
            return false;
        }
#endif
    }
}
