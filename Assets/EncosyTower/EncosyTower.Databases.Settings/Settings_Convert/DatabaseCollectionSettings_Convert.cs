using System.Threading;
using System.Threading.Tasks;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        public Task<ConversionResult[]> ConvertAsync(CancellationToken token, bool continueOnCapturedContext)
            => ConvertAsync(ALL_SOURCES, token, continueOnCapturedContext);

        public async Task<ConversionResult[]> ConvertAsync(
              DataSourceFlags sources
            , CancellationToken token
            , bool continueOnCapturedContext
        )
        {
            var databases = _databases;
            var count = databases.Count;
            var results = new ConversionResult[count];
            var owner = this;

            for (var i = 0; i < count; i++)
            {
                var database = databases[i];
                var result = await database.ConvertAsync(sources, owner, token, continueOnCapturedContext);
                results[i] = new(database.name, result);
            }

            return results;
        }

        public readonly record struct ConversionResult(string DatabaseName, DataSourceFlags ResultFlags);
    }
}
