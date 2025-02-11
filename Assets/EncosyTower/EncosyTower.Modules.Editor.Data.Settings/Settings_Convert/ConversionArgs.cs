using EncosyTower.Modules.Data.Authoring;

namespace EncosyTower.Modules.Editor.Data.Settings
{
    partial class DatabaseCollectionSettings
    {
        internal readonly record struct ConversionArgs(
              string DatabaseAssetName
            , DataSheetContainerBase SheetContainer
            , UnityEngine.Object Owner
        );
    }
}
