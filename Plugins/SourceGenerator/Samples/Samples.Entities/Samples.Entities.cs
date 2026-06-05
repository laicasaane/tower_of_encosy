using Unity.Entities;
using EncosyTower.Entities;

namespace Samples.Entities
{
    [ISystem]
    public class ClassSystemA { }

    [ISystem]
    public class ClassSystemB : ISystem { }

    [ISystem]
    public class ClassSystemC : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }
    }

    [ISystem]
    public struct StructSystemA { }

    [ISystem]
    public struct StructSystemB : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
        }
    }

    [ISystem]
    public struct StructSystemC : ISystem
    {
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
