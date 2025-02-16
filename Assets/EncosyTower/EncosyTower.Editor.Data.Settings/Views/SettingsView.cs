using System;
using EncosyTower.UIElements;
using EncosyTower.Editor.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Data.Settings.Views
{
    using HelpType = HelpBoxMessageType;

    internal abstract class SettingsView : VisualElement
    {
        private const string ACTIVE = "active";
        private const string INACTIVE = "inactive";

        public event Action ValueUpdated;

        protected readonly ViewResources Resources;

        private readonly EnableableFoldout _foldout;
        private readonly Label _subText;
        private readonly VisualElement _subTextContainer;

        public SettingsView(string text, ViewResources resources)
        {
            Resources = resources;

            var foldout = _foldout = new EnableableFoldout(text);
            foldout.Expand = false;
            hierarchy.Add(foldout);

            var subText = _subText = new Label(INACTIVE);
            subText.AddToClassList($"{EnableableFoldout.UssClassName}__sub-text");
            subText.AddToClassList(INACTIVE);

            var subTextContainer = _subTextContainer = new VisualElement();
            subTextContainer.AddToClassList("sub-text-container");
            subTextContainer.Add(subText);

            var foldInput = foldout.Q<VisualElement>(className: EnableableFoldout.InputUssClassName);
            foldInput.Add(subTextContainer);

            foldout.ValueChanged += Foldout_ValueChanged;
        }

        public override VisualElement contentContainer => _foldout.contentContainer;

        public bool Enabled { get; private set; }

        public void BindFoldout(SerializedProperty property)
        {
            _foldout.Bind(property);
            _foldout.Expand = property.boolValue;
        }

        public virtual void Unbind()
        {
            _foldout.Unbind();
        }

        protected void BrowseFolder(ButtonTextField sender)
            => DirectoryAPI.OpenFolderPanel(sender.TextField, "Select A Folder");

        protected abstract void OnEnabled(bool value);

        private void Foldout_ValueChanged(EnableableFoldout _, ChangeEvent<bool> evt)
        {
            var subText = _subText;
            var subTextContainer = _subTextContainer;
            var value = Enabled = evt.newValue;

            var classToRemove = value ? INACTIVE : ACTIVE;
            var classToAdd = value ? ACTIVE : INACTIVE;

            subText.RemoveFromClassList(classToRemove);
            subText.AddToClassList(classToAdd);
            subText.text = value ? ACTIVE : INACTIVE;

            subTextContainer.RemoveFromClassList(classToRemove);
            subTextContainer.AddToClassList(classToAdd);

            OnEnabled(value);
        }

        protected void OnValueChanged_EquatableTyped<TValue>(ChangeEvent<TValue> evt)
            where TValue : IEquatable<TValue>
        {
            if (evt.newValue.Equals(evt.previousValue) == false)
            {
                ValueUpdated?.Invoke();
            }
        }

        protected void OnValueChanged_ComparableUntyped<TValue>(ChangeEvent<TValue> evt)
            where TValue : IComparable
        {
            if (evt.newValue.CompareTo(evt.previousValue) != 0)
            {
                ValueUpdated?.Invoke();
            }
        }

        protected void InitPathField(
              ButtonTextField element
            , Action<ButtonTextField> onClicked
            , PathType pathType
        )
        {
            var info = new Label();

            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("sub-field-info");
            infoContainer.Add(info);

            Add(element.AddToAlignFieldClass());
            Add(infoContainer);

            var icon = EditorAPI.GetIcon("d_folderopened icon", "folderopened icon");
            element.TextField.tooltip = Resources.RelativePath;
            element.Button.iconImage = Background.FromTexture2D(icon.image as Texture2D);
            element.Button.text = "Browse";
            element.Clicked += onClicked;

            element.TextField.RegisterValueChangedCallback(evt => {
                if (evt.newValue.Contains('<'))
                {
                    /// README
                    Notes.ToPreventIllegalCharsExceptionWhenSearch();
                    return;
                }

                info.text = pathType == PathType.File
                    ? DirectoryAPI.ProjectPath.GetFileAbsolutePath(evt.newValue)
                    : DirectoryAPI.ProjectPath.GetFolderAbsolutePath(evt.newValue);
            });
        }

        protected void TryDisplayFolderHelp(
              HelpBox helpBox
            , string relativePath
            , DisplayFolderHelpParams paramsForEmpty
            , DisplayFolderHelpParams paramsForMissing
            , ref bool result
        )
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                helpBox.text = paramsForEmpty.Text;
                helpBox.messageType = paramsForEmpty.Type;
                helpBox.SetDisplay(DisplayStyle.Flex);

                result = paramsForEmpty.Result;
            }
            else
            {
                helpBox.text = paramsForMissing.Text;
                helpBox.messageType = paramsForMissing.Type;

                var exists = DisplayIfFolderNotExist(helpBox, relativePath);

                result = paramsForMissing.Result || exists;
            }
        }

        protected bool DisplayIfStringEmpty(HelpBox helpBox, string value)
        {
            var notEmpty = string.IsNullOrWhiteSpace(value) == false;
            helpBox.SetDisplay(notEmpty ? DisplayStyle.None : DisplayStyle.Flex);

            return notEmpty;
        }

        protected bool DisplayIfFileNotExist(HelpBox helpBox, string relativePath)
        {
            var exists = DirectoryAPI.ProjectPath.ExistsRelativeFile(relativePath);
            helpBox.SetDisplay(exists ? DisplayStyle.None : DisplayStyle.Flex);

            return exists;
        }

        protected bool DisplayIfFolderNotExist(HelpBox helpBox, string relativePath)
        {
            var exists = DirectoryAPI.ProjectPath.ExistsRelativeFolder(relativePath);
            helpBox.SetDisplay(exists ? DisplayStyle.None : DisplayStyle.Flex);

            return exists;
        }

        protected enum PathType
        {
            File,
            Folder,
        }

        protected readonly record struct DisplayFolderHelpParams(string Text, HelpType Type, bool Result);
    }
}
