using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    /// <summary>
    /// Get the path to the file code wherein this attribute is used.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum
        | AttributeTargets.Interface
        | AttributeTargets.Delegate
        , Inherited = false
    )]
    public sealed class ThisFilePathAttribute : Attribute
    {
#if UNITY_EDITOR
        public string FilePath { get; }

        public ThisFilePathAttribute([CallerFilePath][NotNull] string filePath = "")
        {
            FilePath = filePath;
        }
#endif
    }
}
