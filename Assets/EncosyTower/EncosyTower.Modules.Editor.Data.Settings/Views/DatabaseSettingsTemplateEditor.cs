using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Modules.Editor.Data.Settings.Views
{
    [UnityEditor.CustomEditor(typeof(DatabaseSettingsPreset), true)]
    public class DatabaseSettingsTemplateEditor : UnityEditor.Editor
    {
        private DatabaseSettingsView _dbView;

        public override VisualElement CreateInspectorGUI()
        {
            DatabaseTypeVault.Initialize();

            var resources = DatabaseViewAPI.GetResources();
            var target = this.target as DatabaseSettingsPreset;
            var database = target._database;
            var databaseProperty = serializedObject.FindProperty(nameof(DatabaseSettingsPreset._database));
            var context = new DatabaseSettingsContext();
            context.Initialize(database, databaseProperty);

            var root = new VisualElement();
            DatabaseViewAPI.ApplyStyleSheetsTo(root);

            var dbView = _dbView = new DatabaseSettingsView(true, resources) { userData = context };
            dbView.DatabaseTypeSelected += DbView_OnDatabaseTypeSelected;
            dbView.OtherValueUpdated += DbView_OnOtherValueUpdated;
            dbView.Bind(context);

            root.Add(dbView);
            return root;
        }

        public override void OnInspectorGUI()
        {
            _dbView?.Update();
        }

        private void DbView_OnDatabaseTypeSelected(DatabaseSettingsView dbView, DatabaseType type)
        {
            var context = dbView.userData as DatabaseSettingsContext;
            var serializedObject = context.SerializedObject;

            var nameProperty = context.Database.GetNameProperty();
            var typeProperty = context.Database.GetTypeProperty();

            if (type?.Type == null)
            {
                nameProperty.stringValue = Constants.UNDEFINED;
                typeProperty.stringValue = Constants.UNDEFINED;
                dbView.ToggleDisplayContainer(false);
            }
            else
            {
                nameProperty.stringValue = type.Name;
                typeProperty.stringValue = type.Type.AssemblyQualifiedName;
                dbView.ToggleDisplayContainer(true);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void DbView_OnOtherValueUpdated(DatabaseSettingsView dbView)
        {
            var context = dbView.userData as DatabaseSettingsContext;
            var serializedObject = context.SerializedObject;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }
    }
}
