using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Annotations
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
    [Conditional("UNITY_EDITOR"), Conditional("ENCOSY_INCLUDE_AUTHORING")]
    public sealed class ThisFilePathAttribute : Attribute
    {
        public string FilePath { get; }

        /// <inheritdoc cref="ThisFilePathAttribute" />
        public ThisFilePathAttribute([CallerFilePath][NotNull] string filePath = "")
        {
            FilePath = filePath;
        }
    }
}
