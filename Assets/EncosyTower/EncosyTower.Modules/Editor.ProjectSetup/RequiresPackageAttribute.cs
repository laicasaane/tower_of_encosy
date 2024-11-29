using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Modules.Editor.ProjectSetup
{
    /// <summary>
    /// Declares a UPM package dependency.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class RequiresPackageAttribute : Attribute
    {
        public PackageRegistry Registry { get; }

        /// <summary>
        /// Accepts value that is either:
        /// <list type="bullet">
        /// <item>The official name of the package that must conform to
        /// <see href="https://docs.unity3d.com/Manual/cus-naming.html"/>
        /// </item>
        /// <item>The Git URL to the package repo</item>
        /// </list>
        /// </summary>
        public string PackageName { get; }

        public bool IsOptional { get; }

        /// <summary>
        /// Version to append to the package identifier upon calling <c>Client.Add(...)</c>
        /// </summary>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/PackageManager.Client.Add.html"/>
        public string Version { get; }

        public RequiresPackageAttribute(
             PackageRegistry registry
            , [NotNull] string packageName
            , string version = ""
            , bool isOptional = false
        )
        {
            Registry = registry;
            PackageName = packageName;
            Version = version ?? string.Empty;
            IsOptional = isOptional;
        }
    }
}
