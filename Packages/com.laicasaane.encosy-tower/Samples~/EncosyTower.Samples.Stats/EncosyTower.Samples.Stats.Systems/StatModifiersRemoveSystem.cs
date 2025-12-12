using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace EncosyTower.Samples.Stats
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial struct StatModifiersRemoveSystem : ISystem
    {
        private StatSystem.Accessor _accessor;
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _accessor = new(ref state);

            _query = SystemAPI.QueryBuilder()
                .WithDisabled<IsLivingTag>()
                .WithAllRW<ModifierHandle>()
                .Build();

            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _accessor.Update(ref state);

            state.Dependency = new RemoveOutOfTimeModifiersJob {
                accessor = _accessor
            }.ScheduleParallel(_query, state.Dependency);
        }

        [BurstCompile]
        private partial struct RemoveOutOfTimeModifiersJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public StatSystem.Accessor accessor;

            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            private StatSystem.WorldData _worldData;

            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            private NativeList<ModifierHandle> _tempHandles;

            private void Execute(ref DynamicBuffer<ModifierHandle> modifierHandleBuffer)
            {
                if (modifierHandleBuffer.Length < 1)
                {
                    return;
                }

                var accessor = this.accessor;
                var worldData = _worldData;
                var tempHandles = _tempHandles;

                tempHandles.Clear();
                tempHandles.AddRange(modifierHandleBuffer.AsNativeArray());
                modifierHandleBuffer.Clear();

                for (var i = 0; i < tempHandles.Length; i++)
                {
                    var modifierHandle = tempHandles[i];
                    var modifierHandleIndex = modifierHandle.value.affectedStatHandle.index;

                    if (accessor.TryRemoveStatModifier(modifierHandle.value, ref worldData) == false)
                    {
                        continue;
                    }

                    for (var k = i + 1; k < tempHandles.Length; k++)
                    {
                        ref var nextHandleRef = ref tempHandles.ElementAt(k);
                        ref var nextHandleIndex = ref nextHandleRef.value.affectedStatHandle.index;

                        if (nextHandleIndex > modifierHandleIndex)
                        {
                            nextHandleIndex--;
                        }
                    }
                }
            }

            [BurstCompile]
            public bool OnChunkBegin(in ArchetypeChunk chunk, int __, bool ___, in v128 ____)
            {
                if (_worldData.IsCreated == false)
                {
                    _worldData = new(chunk.Count, Allocator.TempJob);
                }

                if (_tempHandles.IsCreated == false)
                {
                    _tempHandles = new NativeList<ModifierHandle>(8, Allocator.TempJob);
                }

                return true;
            }

            [BurstCompile]
            public void OnChunkEnd(in ArchetypeChunk _, int __, bool ___, in v128 ____, bool _____)
            {
                if (_worldData.IsCreated)
                {
                    _worldData.Dispose();
                }

                if (_tempHandles.IsCreated)
                {
                    _tempHandles.Dispose();
                }
            }
        }
    }
}
