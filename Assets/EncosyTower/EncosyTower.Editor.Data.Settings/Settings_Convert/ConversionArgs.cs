using EncosyTower.Data.Authoring;

namespace EncosyTower.Editor.Data.Settings
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
