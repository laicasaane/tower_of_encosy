using UnityEngine;

namespace EncosyTower.Modules.Editor.Data.Settings
{
    [CreateAssetMenu(fileName = "DatabaseSettingsTemplate", menuName = "EncosyTower/Database/Settings Template")]
    public class DatabaseSettingsTemplate : ScriptableObject
    {
        [SerializeField] internal DatabaseCollectionSettings.DatabaseSettings _database;
    }
}
