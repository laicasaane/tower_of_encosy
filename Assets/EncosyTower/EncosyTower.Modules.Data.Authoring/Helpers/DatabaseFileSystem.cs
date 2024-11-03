using System.Collections.Generic;
using System.IO;
using Cathei.BakingSheet.Internal;

namespace EncosyTower.Modules.Data.Authoring
{
    public interface IExtendedFileSystem : IFileSystem
    {
        IEnumerable<string> GetFiles(string path, string extension, bool includeSubFolders);

        void DeleteDirectory(string path, bool recursive = true);

        bool DirectoryExists(string path);
    }

    public class DatabaseFileSystem : FileSystem, IExtendedFileSystem
    {
        public virtual IEnumerable<string> GetFiles(string path, string extension, bool includeSubFolders)
        {
            var option = includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(path, "*." + extension, option);
        }

        public virtual void DeleteDirectory(string path, bool recursive = true)
        {
            Directory.Delete(path, recursive);
        }

        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
