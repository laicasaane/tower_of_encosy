#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditorInternal;

namespace EncosyTower.Editor.AssemblyDefs
{
    public record AssemblyData(
          string AssetPath
        , AssemblyDefinitionAsset Asset
        , AssemblyDefinitionInfo AssemblyDefinition
        , bool UseGuid
        , List<AssemblyReferenceData> AllReferences
        , List<AssemblyReferenceData> FilteredReferences
    );
}

#endif
