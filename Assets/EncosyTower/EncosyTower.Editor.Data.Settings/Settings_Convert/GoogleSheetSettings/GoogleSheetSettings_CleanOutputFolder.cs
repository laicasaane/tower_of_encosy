using System.IO;
using EncosyTower.IO;

namespace EncosyTower.Editor.Data.Settings
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
