using System.IO;
using EncosyTower.Editor;
using EncosyTower.IO;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            public void CleanOutputFolder()
            {
                if (string.IsNullOrWhiteSpace(outputRelativeFolderPath))
                {
                    return;
                }

                var rootPath = new RootPath(EditorAPI.ProjectPath);
                rootPath.DeleteRelativeFolder(outputRelativeFolderPath);
                rootPath.CreateRelativeFolder(outputRelativeFolderPath);
                rootPath.CreateRelativeFile(Path.Combine(outputRelativeFolderPath, ".preserved"));
            }
        }
    }
}
