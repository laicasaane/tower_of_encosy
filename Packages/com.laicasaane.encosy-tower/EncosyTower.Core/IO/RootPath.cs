using System.IO;
using System.Text;

namespace EncosyTower.IO
{
    public readonly record struct RootPath(string Root)
    {
        public static implicit operator RootPath(string value)
            => new(value ?? string.Empty);

        public bool IsValid => string.IsNullOrWhiteSpace(Root) == false;

        /// <summary>
        /// Translates the path to UNIX format which uses <c>/</c> instead of <c>\</c>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ToUnixPath(string path)
                => path?.Replace('\\', '/');

        /// <summary>
        /// Returns the absolute path to the <see cref="Root"/>.
        /// </summary>
        public string GetAbsolutePath()
            => GetFolderAbsolutePath(string.Empty);

        /// <summary>
        /// Returns the absolute path to a file after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <returns></returns>
        public string GetFileAbsolutePath(string relativePath)
        {
            var path = Path.Combine(Root, relativePath ?? string.Empty);
            var fullPath = Path.GetFullPath(path);
            return ToUnixPath(fullPath);
        }

        /// <summary>
        /// Returns the root to a folder (absolute path) after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// If <paramref name="relativePath"/> points to a file, the returned value will be the absolute path of its
        /// containing folder.
        /// </remarks>
        public RootPath GetFolderRoot(string relativePath)
        {
            var fullPath = GetFileAbsolutePath(relativePath);

            return File.Exists(fullPath)
                ? ToUnixPath(Path.GetDirectoryName(fullPath))
                : fullPath;
        }

        /// <summary>
        /// Returns the absolute path to a folder after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// If <paramref name="relativePath"/> points to a file, the returned value will be the absolute path of its
        /// containing folder.
        /// </remarks>
        public string GetFolderAbsolutePath(string relativePath)
        {
            var fullPath = GetFileAbsolutePath(relativePath);

            return File.Exists(fullPath)
                ? ToUnixPath(Path.GetDirectoryName(fullPath))
                : fullPath;
        }

        /// <summary>
        /// Returns the relative path after separating it from <see cref="Root"/>.
        /// </summary>
        /// <param name="absolutePath">The path that is absolute to <see cref="Root"/>.</param>
        /// <returns></returns>
        public string GetRelativePath(string absolutePath)
        {
            var path = Path.GetRelativePath(Root, absolutePath);
            return ToUnixPath(path);
        }

        public bool ExistsRelativeFile(string relativePath)
        {
            var path = GetFileAbsolutePath(relativePath);
            return File.Exists(path);
        }

        public bool ExistsRelativeFolder(string relativePath)
        {
            var path = GetFolderAbsolutePath(relativePath);
            return Directory.Exists(path);
        }

        public void CreateRelativeFile(string relativePath)
        {
            var path = GetFileAbsolutePath(relativePath);
            File.WriteAllText(path, string.Empty, Encoding.UTF8);
        }

        public void CreateRelativeFolder(string relativePath)
        {
            var path = GetFolderAbsolutePath(relativePath);
            Directory.CreateDirectory(path);
        }

        public void DeleteRelativeFile(string relativePath)
        {
            var path = GetFileAbsolutePath(relativePath);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void DeleteRelativeFolder(string relativePath, bool recursive = true)
        {
            var path = GetFolderAbsolutePath(relativePath);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }
    }
}
