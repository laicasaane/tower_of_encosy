using System;

namespace EncosyTower.EnumExtensions.SourceGen
{
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class GeneratedFromEnumTemplateAttribute : Attribute
    {
        public Type TemplateType { get; }

        public GeneratedFromEnumTemplateAttribute(Type templateType)
        {
            TemplateType = templateType;
        }
    }
}
