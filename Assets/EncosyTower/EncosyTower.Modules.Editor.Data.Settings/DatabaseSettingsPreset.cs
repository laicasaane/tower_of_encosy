using UnityEngine;

namespace EncosyTower.Modules.Editor.Data.Settings
{
    [CreateAssetMenu(fileName = "DatabaseSettingsPreset", menuName = "EncosyTower/Database/Settings Preset")]
    public class DatabaseSettingsPreset : ScriptableObject
    {
        [SerializeField] internal DatabaseCollectionSettings.DatabaseSettings _database;
    }
}
