using EncosyTower.EnumExtensions;

namespace EncosyTower.Tests.EnumTemplates
{
    [EnumTemplate]
    [EnumTemplateMemberFromType(typeof(CustomFruit<int>), 500, "Integer Fruit", "IntFruit")]
    public readonly partial struct ResourceType_EnumTemplate { }

    partial class ResourceTypeExtensions { }

    [EnumMembersForTemplate(typeof(ResourceType_EnumTemplate), 000)]
    public enum ResourceNone : byte { None }

    [EnumMembersForTemplate(typeof(ResourceType_EnumTemplate), 100)]
    public enum FruitType : byte { Apple, Orange, }

    [EnumMembersForTemplate(typeof(ResourceType_EnumTemplate), 200)]
    public enum GrainType : byte { Wheat, Rice, }

    public readonly struct CustomFruit<T> { }

    [TypeAsEnumMemberForTemplate(typeof(ResourceType_EnumTemplate), 600, "Special Power Fruit", "SP Fruit")]
    public class SpecialPowerFruit { }
}