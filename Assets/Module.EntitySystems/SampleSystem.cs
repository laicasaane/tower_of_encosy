using Unity.Burst;
using Unity.Entities;

namespace Module.EntitySystems
{
    [BurstCompile]
    public partial struct SampleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }
    }
}
