using UnityEngine;

namespace EncosyTower.Modules
{
    public static class ApplicationAPI
    {
        /// <summary>
        /// The directory name of the current project folder.
        /// </summary>
        public static string GetProjectFolderName()
        {
            var path = Application.dataPath.Split('/');
            return path[^2];
        }
    }
}
