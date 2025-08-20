using EncosyTower.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal sealed class LocalExcelFolderSettingsView : LocalFolderSettingsView
    {
        private TextField _extensionText;

        public LocalExcelFolderSettingsView(
              string text
            , string ussClassName
            , ViewResources resources
            , DataSourceFlags source
        )
            : base(text, ussClassName, resources, source)
        {
        }

        protected override void CreateAdditionalFields()
        {
            _extensionText = new TextField("File Extension") {
                tooltip = "The file extension for Excel files.",
            };

            contentContainer.Add(_extensionText.AddToAlignFieldClass());
        }

        protected override void OnBind(LocalFolderContext context)
        {
            _extensionText.BindProperty(context.GetExtensionProperty());
        }

        protected override void OnUnbind()
        {
            _extensionText.Unbind();
        }
    }
}
