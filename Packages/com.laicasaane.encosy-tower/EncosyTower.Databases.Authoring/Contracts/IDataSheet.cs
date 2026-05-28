using Cathei.BakingSheet;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Provides additional functionality for authoring sheet conversion.
    /// </summary>
    public interface IDataSheet : ISheet
    {
        /// <summary>
        /// Prepare every authoring sheet before subsequent mechanism.
        /// </summary>
        void Initialize(SheetConvertingContext context);

        /// <summary>
        /// Entry point for manual conversion by user code.
        /// </summary>
        void PostConvert(SheetConvertingContext context);
    }
}
