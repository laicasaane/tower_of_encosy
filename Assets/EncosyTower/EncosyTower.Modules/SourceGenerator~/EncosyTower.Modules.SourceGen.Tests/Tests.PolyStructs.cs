using EncosyTower.Modules.PolyStructs;
using UnityEngine;

namespace EncosyTower.Modules.Tests.PolyStructs
{
    [PolyStructInterface]
    public interface ITask
    {
        bool IsValid { get; }

        void DoSomething(int value, in Vector2 position);

        Vector3 Calculate(in Vector3 value);

        bool TryGet(out int value);
    }

    [PolyStruct]
    public partial struct TaskA : ITask
    {
        public bool IsValid => true;

        public void DoSomething(int value, in Vector2 position)
        {
            Debug.Log($"DoSomething: {value}, {position}");
        }

        public Vector3 Calculate(in Vector3 value)
        {
            return value * 2;
        }

        public bool TryGet(out int value)
        {
            value = 42;
            return true;
        }
    }

    [PolyStruct]
    public partial struct TaskB : ITask
    {
        public bool IsValid => false;

        public void DoSomething(int value, in Vector2 position)
        {
            Debug.Log($"DoSomething: {value}, {position}");
        }

        public Vector3 Calculate(in Vector3 value)
        {
            return value * 3;
        }

        public bool TryGet(out int value)
        {
            value = 0;
            return false;
        }
    }

    partial struct Task { }
}
