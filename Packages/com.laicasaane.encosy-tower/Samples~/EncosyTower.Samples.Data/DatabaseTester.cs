using EncosyTower.Databases;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Samples.Data
{
    public class DatabaseTester : MonoBehaviour
    {
        [SerializeField] private DatabaseAsset _database;

        private void Awake()
        {
            var db = new SampleDatabase(_database, InitializationBehaviour.Forced);
            var heroEntries = db.Heroes.Entries;
            var span = heroEntries.Span;

            foreach (var entry in span)
            {
                StaticDevLogger.LogInfo(entry);

                var multipliers = entry.Multipliers.Span;

                foreach (var multiplier in multipliers)
                {
                    StaticDevLogger.LogInfo(multiplier);
                }
            }
        }
    }
}
