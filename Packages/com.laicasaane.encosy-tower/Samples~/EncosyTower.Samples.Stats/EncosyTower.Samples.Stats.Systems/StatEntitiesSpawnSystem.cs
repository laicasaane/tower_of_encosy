using EncosyTower.Collections;
using EncosyTower.Entities;
using EncosyTower.Entities.Stats;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace EncosyTower.Samples.Stats
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial struct StatEntitiesSpawnSystem : ISystem
    {
        private StatSystem.Accessor _accessor;
        private EntityQuery _queryCommands;
        private Lookups _lookups;
        private BufferLookup<ModifierHandle> _lookupModifierHandles;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _accessor = new(ref state);
            _lookups = new(ref state);
            _lookupModifierHandles = state.GetBufferLookup<ModifierHandle>();

            _queryCommands = SystemAPI.QueryBuilder().WithAll<StatSpawnCommand>().Build();

            state.RequireForUpdate<StatPrefabRef>();
            state.RequireForUpdate<AffectorPrefabRef>();
            state.RequireForUpdate<RandomRef>();
            state.RequireForUpdate(_queryCommands);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commands = _queryCommands.ToComponentDataArray<StatSpawnCommand>(Allocator.Temp);

            state.EntityManager.DestroyEntity(_queryCommands);
            state.Dependency = default;

            var statAmount = 0;
            var affectorAmount = 0;

            foreach (var command in commands)
            {
                statAmount += command.statAmount;
                affectorAmount += command.affectorAmount;
            }

            if (statAmount < 1)
            {
                return;
            }

            var allocator = state.WorldUpdateAllocator;
            var statPrefab = SystemAPI.GetSingleton<StatPrefabRef>().value;
            var primaryEntities = state.EntityManager.Instantiate(statPrefab, statAmount, allocator);

            if (affectorAmount < 1)
            {
                return;
            }

            var affectorPrefab = SystemAPI.GetSingleton<AffectorPrefabRef>().value;
            var affectorEntities = state.EntityManager.Instantiate(affectorPrefab, affectorAmount, allocator);
            var random = SystemAPI.GetSingleton<RandomRef>().value;

            _accessor.Update(ref state);
            _lookups.Update(ref state);
            _lookupModifierHandles.Update(ref state);

            state.Dependency = new InitializeAffectorStatsJob {
                accessor = _accessor,
                lookupIndices = _lookups,
                lookupLifetime = _lookups,
                entities = affectorEntities.AsReadOnly(),
                random = new Random(random.NextUInt() + 1),
            }.ScheduleParallel(affectorEntities.Length, 64, state.Dependency);

            state.Dependency = new LinkAffectorsToPrimaryEntitiesJob {
                accessor = _accessor,
                lookups = _lookups,
                lookupModifierHandles = _lookupModifierHandles,
                primaryEntities = primaryEntities.AsReadOnly(),
                affectorEntities = affectorEntities.AsReadOnly(),
                random = new Random(random.NextUInt() + 1),
            }.Schedule(state.Dependency);
        }

        [Lookup(typeof(PrimaryStatIndices), true)]
        [Lookup(typeof(AffectorStatIndices), true)]
        [Lookup(typeof(Lifetime))]
        private partial struct Lookups : IComponentLookups { }

        [BurstCompile]
        private partial struct InitializeAffectorStatsJob : IJobParallelForBatch
        {
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public StatSystem.Accessor accessor;

            [NativeDisableParallelForRestriction, ReadOnly]
            public ComponentLookup<AffectorStatIndices> lookupIndices;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<Lifetime> lookupLifetime;

            public NativeArray<Entity>.ReadOnly entities;
            public Random random;

            public void Execute(int startIndex, int count)
            {
                var end = startIndex + count;
                var lookupIndices = this.lookupIndices;
                var lookupLifetime = this.lookupLifetime;
                var accessor = this.accessor;
                var entities = this.entities;
                var worldData = new StatSystem.WorldData(count, Allocator.Temp);
                var random = new Random(this.random.NextUInt((uint)startIndex, (uint)end) + 1);

                for (var i = startIndex; i < end; i++)
                {
                    var entity = entities[i];
                    var statIndices = lookupIndices[entity].value;

                    accessor.TrySetStatBaseValue(new(entity, statIndices.hp), 200f, ref worldData);
                    accessor.TrySetStatBaseValue(new(entity, statIndices.moveSpeed), 100f, ref worldData);

                    var lifeTimeRef = lookupLifetime.GetRefRW(entity);
                    lifeTimeRef.ValueRW.value = random.NextFloat(1f, 6f);
                }
            }
        }

        [BurstCompile]
        private partial struct LinkAffectorsToPrimaryEntitiesJob : IJob
        {
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public StatSystem.Accessor accessor;

            [NativeDisableParallelForRestriction]
            public Lookups lookups;

            [NativeDisableParallelForRestriction]
            public BufferLookup<ModifierHandle> lookupModifierHandles;

            /// <summary>
            /// Primary entities represent the characters, or main objects of a game.
            /// For this strategy, each of them has a buffer of <see cref="StatSystem.Stat"/> of their own.
            /// </summary>
            public NativeArray<Entity>.ReadOnly primaryEntities;

            /// <summary>
            /// Afftector entities represent the status effects which affect the primary entities.
            /// Each of them has a buffer of <see cref="StatSystem.Stat"/>.
            /// </summary>
            public NativeArray<Entity>.ReadOnly affectorEntities;

            public Random random;

            /// <summary>
            /// Establish a link between a affector entity and a random primary entity, so that when a stat on
            /// the affector changes, it will affect the primary entity.
            /// </summary>
            [BurstCompile]
            public void Execute()
            {
                var lookups = this.lookups;
                var lookupModifierHandles = this.lookupModifierHandles;

                var statOps = NativeArray.CreateFast<StatOp>(3, Allocator.Temp);
                statOps[0] = StatOp.Add;
                statOps[1] = StatOp.AddMultiplier;
                statOps[2] = StatOp.MultiplyMultiplier;

                var worldData = new StatSystem.WorldData(primaryEntities.Length, Allocator.Temp);
                var statEntities = new NativeList<Entity>(primaryEntities.Length, Allocator.Temp);
                statEntities.AddRange(primaryEntities);

                var modifierHandles = new NativeList<ModifierHandle>((int)StatIndices.COUNT, Allocator.Temp);

                for (var i = 0; i < affectorEntities.Length; i++)
                {
                    var affectorEntity = affectorEntities[i];
                    var statEntityIndex = (int)random.NextUInt((uint)statEntities.Length);
                    var statEntity = statEntities[statEntityIndex];

                    // RemoveAtSwapBack [is] an optimization to avoid the cost of shifting elements when remove at
                    // any position but the last. It also introduces a degree of randomness into the list at zero cost.
                    // In this context we don't need the list to stay ordered, but to be randomized as much as possible.
                    statEntities.RemoveAtSwapBack(statEntityIndex);

                    lookups.GetComponentData(statEntity, out PrimaryStatIndices primaryIndices);
                    lookups.GetComponentData(affectorEntity, out AffectorStatIndices affectorIndices);

                    modifierHandles.Clear();

                    var linkCount = (int)random.NextUInt(StatIndices.COUNT) + 1;

                    for (var k = 0; k < linkCount; k++)
                    {
                        var handleIndex = (int)random.NextUInt(StatIndices.LAST_STAT_OF_TYPE_HALF + 1);

                        if (primaryIndices.value.TryGetHandle(handleIndex, out var primaryIndex) == false
                            || affectorIndices.value.TryGetHandle(handleIndex, out var affectorIndex) == false
                        )
                        {
                            continue;
                        }

                        var primaryHandle = new StatHandle(statEntity, primaryIndex);
                        var affectorHandle = new StatHandle(affectorEntity, affectorIndex);
                        var statOpIndex = (int)random.NextUInt((uint)statOps.Length);
                        var statOp = statOps[statOpIndex];

                        if (accessor.TryAddStatModifier(
                              primaryHandle
                            , new StatSystem.StatModifier(statOp, new StatModifierSource(affectorHandle))
                            , out var modifierHandle
                            , ref worldData
                        ))
                        {
                            modifierHandles.Add(new ModifierHandle { value = modifierHandle });
                        }
                    }

                    if (modifierHandles.Length > 0
                        && lookupModifierHandles.TryGetBuffer(affectorEntity, out var modifierHandleBuffer)
                    )
                    {
                        modifierHandleBuffer.AddRange(modifierHandles.AsArray());
                    }
                }
            }
        }
    }
}
