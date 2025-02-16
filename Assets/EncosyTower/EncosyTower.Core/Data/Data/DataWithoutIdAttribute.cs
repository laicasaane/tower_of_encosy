using System;

namespace EncosyTower.Data
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class DataWithoutIdAttribute : Attribute { }
}
