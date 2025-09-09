﻿using System;
using System.Collections;
using EncosyTower.Types;

namespace EncosyTower.Tests.RuntimeTypeCaches
{
    public class YourType { }

    public class SomeAttribute : Attribute { }

    public class SpecialAttribute : Attribute { }

    public partial class Tests
    {
        public void DoSomethingX()
        {
            const string ASSEMBLY = "EncosyTower.Tests";
            RuntimeTypeCache.GetTypesDerivedFrom<IEqualityComparer>(ASSEMBLY);
            RuntimeTypeCache.GetFieldsWithAttribute<SomeAttribute>();
            RuntimeTypeCache.GetInfo<SpecialAttribute>();
        }
    }

    public partial class Tests
    {
        public void DoSomethingY()
        {
            RuntimeTypeCache.GetTypesDerivedFrom<YourType>();
            RuntimeTypeCache.GetFieldsWithAttribute<SpecialAttribute>();
            RuntimeTypeCache.GetFieldsWithAttribute<UnityEngine.SerializeField>("EncosyTower.Tests");
        }
    }
}
