using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.UIElements
{
    public interface IHasBindingPath
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Conforms to Unity naming style.")]
        string bindingPath { get; set; }
    }
}
