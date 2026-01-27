using System.Collections.Generic;
using EncosyTower.NewtonsoftJson;
using UnityEngine;

namespace EncosyTower.Tests.NewtonsoftJsonHelperTests
{
    [NewtonsoftJsonAotHelper(typeof(MyClass))]
    public static partial class Helper { }

    public class MyClass
    {
        public HashSet<int> data;

        public Dictionary<KeyCode, int> data2;

        public MyDict data3;
    }

    public class MyDict : Dictionary<int, int> { }
}
