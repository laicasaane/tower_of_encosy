#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;

namespace EncosyTower.Editor.AssemblyDefs
{
    public class AssemblyReferenceData
    {
        public GUID guid;
        public AssemblyDefinitionAsset asset;
        public string name;
        public string guidString;
        public string headerText;
        public bool selected;

        public string Name => asset ? asset.name : string.Empty;

        public bool IsHeader { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareName(AssemblyReferenceData x, AssemblyReferenceData y)
            => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
    }
}

#endif
