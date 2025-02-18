using System.Diagnostics;

namespace EncosyTower.Databases.Settings.Views
{
    internal static class Notes
    {
        [Conditional("__UNDEFINED__")]
        public static void ToPreventIllegalCharsExceptionWhenSearch()
        {
            // NOTE 1:
            // When performing search in the Project Settings window
            // `evt.newValue` will contain something like `<color></color>`
            // to highlight the search results.
            //
            // However `<` and `>` are illegal characters for directory path
            // this `Path.Combine` will throw exceptions.
            //
            // That's why we should return if we detect a `<` in `evt.newValue`.

            // NOTE 2:
            // Exhausting detection using `Path.GetInvalidPathChars()`
            // might negatively affect the performance.
        }

        [Conditional("__UNDEFINED__")]
        public static void ToPreventTwoWayBindingWhenSearch()
        {
            // When performing search in the Project Settings window,
            // matching texts will be highlighted.
            // To achieve this, Unity replaced the original plain texts
            // with rich texts that contains `<color>` and `<mark>` tags.
            //
            // However, this will trigger the value change events on Label elements.
            // If Label is bound to a SerializedProperty using `.BindProperty()` method
            // this value change will be applied back to the property.
            // Because the default mode of binding is two-way binding: UI element <=> data source.
            //
            // To prevent this, we have to:
            // 1. Use one-way binding through `.TrackPropertyValue()` method.
            // 2. Cast the Label to `INotifyValueChanged<string>` and use its `.SetValueWithoutNotify()` method.
        }
    }
}
