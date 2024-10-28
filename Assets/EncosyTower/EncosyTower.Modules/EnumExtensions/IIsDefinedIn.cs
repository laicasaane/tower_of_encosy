namespace EncosyTower.Modules.EnumExtensions
{
    public interface IIsDefinedIn
    {
        bool IsDefinedIn(string name);

        bool IsDefinedIn(string name, bool allowMatchingMetadataAttribute);
    }
}
