using Cathei.BakingSheet;
using Microsoft.Extensions.Logging;

namespace EncosyTower.Databases.Authoring
{
    public interface IPostExportDatabase
    {
        void PostExport(DatabaseExportingContext context);
    }

    public readonly record struct DatabaseExportingContext(
          DatabaseAsset Asset
        , SheetContainerBase Container
        , ILogger Logger
    );
}
