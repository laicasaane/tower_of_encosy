using System;
using EncosyTower.Modules.Types;

namespace EncosyTower.Modules.Tests.RuntimeTypeCaches
{
    public interface IMyInterface { }

    public class YourType { }

    public class SomeAttribute : Attribute { }

    public partial class Tests
    {
        public void DoSomethingX()
        {
            RuntimeTypeCache.GetTypesDerivedFrom<IMyInterface>("aa");
            RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
            RuntimeTypeCache.GetFieldsWithAttribute<UnityEngine.SerializeField>();
        }
    }

    public partial class Tests
    {
        public void DoSomethingY()
        {
            RuntimeTypeCache.GetTypesDerivedFrom<IMyInterface>("aa");
            RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
            RuntimeTypeCache.GetFieldsWithAttribute<UnityEngine.SerializeField>();
        }
    }
}
