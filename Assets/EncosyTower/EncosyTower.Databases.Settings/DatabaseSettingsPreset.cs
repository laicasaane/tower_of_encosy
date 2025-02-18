using UnityEngine;

namespace EncosyTower.Databases.Settings
{
    [CreateAssetMenu(fileName = "DatabaseSettingsPreset", menuName = "EncosyTower/Database/Settings Preset")]
    public class DatabaseSettingsPreset : ScriptableObject
    {
        [SerializeField] internal DatabaseCollectionSettings.DatabaseSettings _database;
    }
}
