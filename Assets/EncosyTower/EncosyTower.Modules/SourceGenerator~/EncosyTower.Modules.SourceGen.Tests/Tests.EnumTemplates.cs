using EncosyTower.Modules.EnumExtensions;

namespace EncosyTower.Modules.Tests.EnumTemplates
{
    [EnumTemplate]
    [EnumTemplateMemberFromTypeName(typeof(CustomFruit<int>), 500)]
    public readonly partial struct ResourceType_EnumTemplate { }

    [EnumMembersForTemplate(typeof(ResourceType_EnumTemplate), 000)]
    public enum ResourceNone : byte { None }

    [EnumMembersForTemplate(typeof(ResourceType_EnumTemplate), 100)]
    public enum FruitType : byte { Apple, Orange, }

    [EnumMembersForTemplate(typeof(ResourceType_EnumTemplate), 200)]
    public enum GrainType : byte { Wheat, Rice, }

    //[TypeNameMemberForEnumTemplate(typeof(ResourceType_EnumTemplate), 500)]
    public readonly struct CustomFruit<T> { }
}