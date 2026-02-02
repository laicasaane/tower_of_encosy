namespace EncosyTower.Common
{
    public interface IIsNameDefined
    {
        bool IsNameDefined(string name);

        bool IsNameDefined(string name, bool allowMatchingMetadataAttribute);
    }
}
