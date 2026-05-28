using Cathei.BakingSheet;

namespace EncosyTower.Databases.Authoring
{
    public interface IDataSheet : ISheet
    {
        void Initialize(SheetConvertingContext context);
    }
}
