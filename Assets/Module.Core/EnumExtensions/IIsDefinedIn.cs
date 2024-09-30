namespace Module.Core.EnumExtensions
{
    public interface IIsDefinedIn
    {
        bool IsDefinedIn(string name);

        bool IsDefinedIn(string name, bool allowMatchingMetadataAttribute);
    }
}
