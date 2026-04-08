namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct ModelOptions
    {
        public readonly ModelParts Parts;
        public readonly bool IncludeNonPublic;
        public readonly bool IncludeCompilerGenerated;

        public ModelOptions(
              ModelParts parts = ModelParts.All
            , bool includeNonPublic = true
            , bool includeCompilerGenerated = false
        )
        {
            Parts = parts;
            IncludeNonPublic = includeNonPublic;
            IncludeCompilerGenerated = includeCompilerGenerated;
        }
    }
}
