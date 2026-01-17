using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace EncosyTower.IO
{
    public readonly record struct RootPath([NotNull] string Root)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator RootPath(string value)
            => new(value ?? string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(RootPath rootPath)
            => rootPath.Root;

        /// <summary>
        /// Indicates whether the <see cref="Root"/> is a valid rooted path.
        /// </summary>
        /// <seealso cref="Path.IsPathRooted(string)"/>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Path.IsPathRooted(Root);
        }

        /// <summary>
        /// Indicates whether the <see cref="Root"/> is a fully qualified path.
        /// </summary>
        /// <seealso cref="Path.IsPathFullyQualified(string)"/>
        public bool IsFullyQualified
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Path.IsPathFullyQualified(Root);
        }

        /// <summary>
        /// Translates the path to UNIX format which uses <c>/</c> instead of <c>\</c>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToUnixPath(string path)
            => path?.Replace('\\', '/');

        /// <summary>
        /// Returns the absolute path to the <see cref="Root"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetAbsolutePath()
            => GetFolderAbsolutePath(string.Empty);

        /// <summary>
        /// Returns the absolute path to a file after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// The returned path will be in UNIX format.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetFileAbsolutePath(string relativePath)
            => GetFileAbsolutePath(relativePath, unixFormat: true);

        /// <summary>
        /// Returns the absolute path to a file after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <param name="unixFormat">If set to <c>true</c>, the returned path will be in UNIX format.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetFileAbsolutePath(string relativePath, bool unixFormat)
        {
            var path = Path.Combine(Root, relativePath ?? string.Empty);
            var fullPath = Path.GetFullPath(path);
            return ChooseFormat(fullPath, unixFormat);
        }

        /// <summary>
        /// Returns the root to a folder (absolute path) from <see cref="Root"/>.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If <see cref="Root"/> points to a file, the returned value will be the absolute path of its
        /// containing folder.
        /// <br/>
        /// The returned path will be in UNIX format.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RootPath GetFolderRoot()
            => GetFolderRoot(unixFormat: true);

        /// <summary>
        /// Returns the root to a folder (absolute path) from <see cref="Root"/>.
        /// </summary>
        /// <param name="unixFormat">If set to <c>true</c>, the returned path will be in UNIX format.</param>
        /// <returns></returns>
        /// <remarks>
        /// If <see cref="Root"/> points to a file, the returned value will be the absolute path of its
        /// containing folder.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RootPath GetFolderRoot(bool unixFormat)
        {
            var fullPath = GetFileAbsolutePath(Root);

            return File.Exists(fullPath)
                ? ChooseFormat(Path.GetDirectoryName(fullPath), unixFormat)
                : fullPath;
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
        /// <br/>
        /// The returned path will be in UNIX format.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RootPath GetFolderRoot(string relativePath)
            => GetFolderRoot(relativePath, unixFormat: true);

        /// <summary>
        /// Returns the root to a folder (absolute path) after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <param name="unixFormat">If set to <c>true</c>, the returned path will be in UNIX format.</param>
        /// <returns></returns>
        /// <remarks>
        /// If <paramref name="relativePath"/> points to a file, the returned value will be the absolute path of its
        /// containing folder.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RootPath GetFolderRoot(string relativePath, bool unixFormat)
        {
            var fullPath = GetFileAbsolutePath(relativePath);

            return File.Exists(fullPath)
                ? ChooseFormat(Path.GetDirectoryName(fullPath), unixFormat)
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
        /// <br/>
        /// The returned path will be in UNIX format.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetFolderAbsolutePath(string relativePath)
            => GetFolderAbsolutePath(relativePath, unixFormat: true);

        /// <summary>
        /// Returns the absolute path to a folder after combining <paramref name="relativePath"/>
        /// with <see cref="Root"/>.
        /// </summary>
        /// <param name="relativePath">The path that is relative to <see cref="Root"/>.</param>
        /// <param name="unixFormat">If set to <c>true</c>, the returned path will be in UNIX format.</param>
        /// <returns></returns>
        /// <remarks>
        /// If <paramref name="relativePath"/> points to a file, the returned value will be the absolute path of its
        /// containing folder.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetFolderAbsolutePath(string relativePath, bool unixFormat)
        {
            var fullPath = GetFileAbsolutePath(relativePath);

            return File.Exists(fullPath)
                ? ChooseFormat(Path.GetDirectoryName(fullPath), unixFormat)
                : fullPath;
        }

        /// <summary>
        /// Returns the relative path after separating it from <see cref="Root"/>.
        /// </summary>
        /// <param name="absolutePath">The path that is absolute to <see cref="Root"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// The returned path will be in UNIX format.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetRelativePath(string absolutePath)
            => GetRelativePath(absolutePath, unixFormat: true);

        /// <summary>
        /// Returns the relative path after separating it from <see cref="Root"/>.
        /// </summary>
        /// <param name="absolutePath">The path that is absolute to <see cref="Root"/>.</param>
        /// <param name="unixFormat">If set to <c>true</c>, the returned path will be in UNIX format.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetRelativePath(string absolutePath, bool unixFormat)
        {
            var path = Path.GetRelativePath(Root, absolutePath);
            return ChooseFormat(path, unixFormat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistsRelativeFile(string relativePath)
        {
            var path = GetFileAbsolutePath(relativePath);
            return File.Exists(path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistsRelativeFolder(string relativePath)
        {
            var path = GetFolderAbsolutePath(relativePath);
            return Directory.Exists(path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateRelativeFile(string relativePath)
        {
            var path = GetFileAbsolutePath(relativePath);
            File.WriteAllText(path, string.Empty, Encoding.UTF8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ChooseFormat(string path, bool unixFormat)
            => unixFormat ? ToUnixPath(path) : path;
    }
}
