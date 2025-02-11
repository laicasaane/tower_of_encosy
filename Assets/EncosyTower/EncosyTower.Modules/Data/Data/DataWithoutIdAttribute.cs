using System;

namespace EncosyTower.Modules.Data
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class DataWithoutIdAttribute : Attribute { }
}
