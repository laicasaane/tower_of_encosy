using EncosyTower.Editor;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    public class FolderTextField : ButtonTextField
    {
        private const string USS_CLASS_NAME = "folder-text-field";

        public FolderTextField(string textLabel)
            : base(textLabel, default)
        {
            AddToClassList(USS_CLASS_NAME);

            var icon = EditorAPI.GetIcon("d_pick", "pick");
            var iconImage = Background.FromTexture2D(icon.image as Texture2D);

            var button = new Button(iconImage, LocateFolder) {
                tooltip = "Locate Selected Template"
            };

            button.AddToClassList(Constants.ICON_BUTTON);
            hierarchy.Add(button);
        }

        private void LocateFolder()
        {
            var path = TextField.value;

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset.IsValid())
            {
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }
    }
}
