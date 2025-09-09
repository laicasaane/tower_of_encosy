using EncosyTower.Databases.Authoring;

namespace EncosyTower.Databases.Settings
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
