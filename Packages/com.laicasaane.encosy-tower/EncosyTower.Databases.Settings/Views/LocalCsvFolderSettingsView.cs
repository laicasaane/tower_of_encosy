using EncosyTower.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal sealed class LocalCsvFolderSettingsView : LocalFolderSettingsView
    {
        private Toggle _splitHeaderToggle;
        private TextField _extensionText;

        public LocalCsvFolderSettingsView(
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
            _splitHeaderToggle = new Toggle("Split Header?") {
                tooltip = "If enabled, the header is a combination of multiple consecutive rows."
            };

            contentContainer.Add(_splitHeaderToggle.WithAlignFieldClass());

            _extensionText = new TextField("File Extension") {
                tooltip = "The file extension for Excel files.",
            };

            contentContainer.Add(_extensionText.WithAlignFieldClass());
        }

        protected override void OnBind(LocalFolderContext context)
        {
            _splitHeaderToggle.BindProperty(context.GetSplitHeaderProperty());
            _extensionText.BindProperty(context.GetExtensionProperty());
        }

        protected override void OnUnbind()
        {
            _splitHeaderToggle.Unbind();
            _extensionText.Unbind();
        }
    }
}
