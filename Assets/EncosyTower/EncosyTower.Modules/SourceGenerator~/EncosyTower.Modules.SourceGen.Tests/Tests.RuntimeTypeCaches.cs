using System;
using System.Collections;
using EncosyTower.Modules.Types;

namespace EncosyTower.Modules.Tests.RuntimeTypeCaches
{
    public class YourType { }

    public class SomeAttribute : Attribute { }

    public class SpecialAttribute : Attribute { }

    public partial class Tests
    {
        public void DoSomethingX()
        {
            var a = "EncosyTower.Modules.Tests";
            RuntimeTypeCache.GetTypesDerivedFrom<IEqualityComparer>(a);
            RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
        }
    }

    public partial class Tests
    {
        public void DoSomethingY()
        {
            RuntimeTypeCache.GetTypesDerivedFrom<YourType>();
            RuntimeTypeCache.GetFieldsWithAttribute<SpecialAttribute>();
            RuntimeTypeCache.GetFieldsWithAttribute<UnityEngine.SerializeField>("EncosyTower.Modules.Tests");
        }
    }
}
