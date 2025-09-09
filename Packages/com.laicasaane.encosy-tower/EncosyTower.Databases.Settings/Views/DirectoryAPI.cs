using EncosyTower.Editor;
using EncosyTower.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal static class DirectoryAPI
    {
        public static RootPath ProjectPath => EditorAPI.ProjectPath;

        public static void OpenFilePanel(TextField textField, string title, string[] filters)
        {
            var oldPath = ProjectPath.GetFileAbsolutePath(textField.value);
            var newPath = EditorUtility.OpenFilePanelWithFilters(title, oldPath, filters);

            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            textField.value = ProjectPath.GetRelativePath(newPath);
        }

        public static void OpenFolderPanel(TextField textField, string title)
        {
            var oldPath = ProjectPath.GetFolderAbsolutePath(textField.value);
            var newPath = EditorUtility.OpenFolderPanel(title, oldPath, string.Empty);

            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            textField.value = ProjectPath.GetRelativePath(newPath);
        }
    }
}
