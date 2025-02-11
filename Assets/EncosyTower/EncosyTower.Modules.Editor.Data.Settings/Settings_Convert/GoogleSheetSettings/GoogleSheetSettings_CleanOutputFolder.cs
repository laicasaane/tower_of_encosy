using System.IO;
using EncosyTower.Modules.IO;

namespace EncosyTower.Modules.Editor.Data.Settings
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
