using System;
using EncosyTower.Modules.Types;

namespace EncosyTower.Modules.Tests.RuntimeTypeCaches
{
    public interface IMyInterface { }

    public class YourType { }

    public class SomeAttribute : Attribute { }

    public class Tests
    {
        public void DoSomething()
        {
            RuntimeTypeCache.GetTypesDerivedFrom<IMyInterface>();
            RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
            RuntimeTypeCache.GetFieldsWithAttribute<UnityEngine.SerializeField>();
        }
    }
}
