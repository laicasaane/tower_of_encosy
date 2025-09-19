namespace EncosyTower.EnumExtensions
{
    public interface IIsNameDefined
    {
        bool IsNameDefined(string name);

        bool IsNameDefined(string name, bool allowMatchingMetadataAttribute);
    }
}
