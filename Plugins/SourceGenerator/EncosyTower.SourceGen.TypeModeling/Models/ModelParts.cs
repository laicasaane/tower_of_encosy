using System;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    [Flags]
    public enum ModelParts
    {
        None          = 0,
        Fields        = 1 << 0,
        Properties    = 1 << 1,
        Methods       = 1 << 2,
        Constructors  = 1 << 3,
        Events        = 1 << 4,
        Attributes    = 1 << 5,
        Interfaces    = 1 << 6,
        AllInterfaces = 1 << 7,
        All           = ~0,
    }
}