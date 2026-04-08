using System;

namespace EncosyTower.SourceGen.TypeModeling
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
        All           = ~0,
    }

    public readonly struct ModelOptions
    {
        public readonly ModelParts Parts;
        public readonly bool IncludeNonPublic;
        public readonly bool IncludeCompilerGenerated;

        public ModelOptions(
            ModelParts parts = ModelParts.All,
            bool includeNonPublic = true,
            bool includeCompilerGenerated = false)
        {
            Parts = parts;
            IncludeNonPublic = includeNonPublic;
            IncludeCompilerGenerated = includeCompilerGenerated;
        }
    }
}
