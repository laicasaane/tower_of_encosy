#if UNITY_LOCALIZATION

using System;

namespace EncosyTower.Modules.Localization
{
    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// Place on a struct whose name ends with either "_EnumTemplate" or "_Template"
    /// to indicate a template for generating an L10n Language enum.
    /// </summary>
    /// <example>
    /// <code>
    /// [L10nLanguageEnumTemplate]
    /// public readonly partial struct GameLanguage_Template { }
    ///
    /// // Will generate an enum named 'GameLanguage' along with associated APIs.
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class L10nLanguageEnumTemplateAttribute : Attribute { }
}

#endif
