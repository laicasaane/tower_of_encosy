using System;
using EncosyTower.Entities.Stats;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace EncosyTower.Samples.Stats
{
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial class HudSpawnEventHandleSystem : SystemBase
    {
        private SpawnMessage? _msg;
        private EntityArchetype _commandArchetype;

        protected override void OnCreate()
        {
            var types = new NativeArray<ComponentType>(1, Allocator.Temp);
            types[0] = ComponentType.ReadWrite<StatSpawnCommand>();

            _commandArchetype = EntityManager.CreateArchetype(types);
        }

        protected override void OnStartRunning()
        {
            HudEvents.OnSpawnStat += Handle;
        }

        protected override void OnStopRunning()
        {
            HudEvents.OnSpawnStat -= Handle;
        }

        protected override void OnUpdate()
        {
            if (_msg.HasValue == false)
            {
                return;
            }

            var (statAmount, affectedPercent) = _msg.Value;
            _msg = null;

            if (statAmount < 1)
            {
                return;
            }

            var affectedAmount = (int)(statAmount * affectedPercent);
            var entity = EntityManager.CreateEntity(_commandArchetype);

            EntityManager.SetComponentData(entity, new StatSpawnCommand {
                statAmount = statAmount,
                affectorAmount = affectedAmount,
            });
        }

        private void Handle(SpawnMessage msg)
        {
            _msg = msg;
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial class StatOwnerCounterUpdateSystem : SystemBase
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = SystemAPI.QueryBuilder().WithAll<StatOwner>().Build();

            RequireForUpdate(_query);
        }

        protected override void OnUpdate()
        {
            var count = _query.CalculateEntityCount();
            HudEvents.UpdateCounter(count);
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial class RandomUpdateSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var types = new NativeArray<ComponentType>(1, Allocator.Temp);
            types[0] = ComponentType.ReadWrite<RandomRef>();

            var archetype = EntityManager.CreateArchetype(types);
            var entity = EntityManager.CreateEntity(archetype);

            EntityManager.SetComponentData(entity, new RandomRef {
                value = Random.CreateFromIndex(
                    (uint)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
                )
            });

            RequireForUpdate<RandomRef>();
        }

        protected override void OnUpdate()
        {
            var refRW = SystemAPI.GetSingletonRW<RandomRef>();
            refRW.ValueRW.value = Random.CreateFromIndex(refRW.ValueRO.value.NextUInt());
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LifetimeElapseSystem : ISystem
    {
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = SystemAPI.QueryBuilder()
                .WithAllRW<Lifetime>()
                .WithAllRW<IsLivingTag>()
                .Build();

            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ElapseLifetimeJob {
                deltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(_query, state.Dependency);
        }

        [BurstCompile]
        private partial struct ElapseLifetimeJob : IJobEntity
        {
            public float deltaTime;

            private void Execute(ref Lifetime lifetime, EnabledRefRW<IsLivingTag> isLivingRef)
            {
                var time = lifetime.value -= deltaTime;
                isLivingRef.ValueRW = time > 0f;
            }
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial struct DeadEntityDestroySystem : ISystem
    {
        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = SystemAPI.QueryBuilder()
                .WithDisabled<IsLivingTag>()
                .Build();

            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = _query.ToEntityArray(Allocator.Temp);
            state.EntityManager.DestroyEntity(entities);

            state.Dependency = default;
        }
    }
}
