using Cathei.BakingSheet;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Provides additional functionality for authoring sheet conversion.
    /// </summary>
    public interface IDataSheet : ISheet
    {
        /// <summary>
        /// Entry point for the pre-processing step by user code.
        /// </summary>
        void Preprocess(SheetConvertingContext context);

        /// <summary>
        /// Entry point for the processing step by user code.
        /// </summary>
        void Process(SheetConvertingContext context);

        /// <summary>
        /// Entry point for the post=processing step by user code.
        /// </summary>
        void Postprocess(SheetConvertingContext context);
    }
}
