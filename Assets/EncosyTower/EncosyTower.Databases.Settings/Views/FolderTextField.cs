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
            : this(textLabel, default)
        {
        }

        public FolderTextField(string textLabel, Background iconImage)
            : base(textLabel, iconImage)
        {
            AddToClassList(USS_CLASS_NAME);

            var pickIcon = EditorAPI.GetIcon("d_pick", "pick");
            var pickImage = Background.FromTexture2D(pickIcon.image as Texture2D);

#if UNITY_6000_0_OR_NEWER
            var button = new Button(pickImage, LocateFolder);
#else
            var button = ButtonAPI.CreateButton(pickImage, LocateFolder);
            button.AddToClassList(ButtonAPI.IconOnlyUssClassName);
#endif

            button.tooltip = "Locate Selected Template";
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
