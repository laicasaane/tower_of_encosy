#if UNITY_ENTITIES

using Unity.Entities;

namespace EncosyTower.Entities.Contracts
{
    public interface ISystemStateUpdate
    {
        public void Update(ref SystemState state) { }
    }

    public interface ISystemBaseUpdate
    {
        public void Update(SystemBase system) { }
    }
}

#endif
