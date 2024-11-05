using EncosyTower.Modules.EnumExtensions;

namespace Module.GameCommon
{
    /// <summary>Character Animation</summary>
    [EnumTemplate]
    public readonly partial struct CharAnim_Template { }

    [EnumMembersForTemplate(typeof(CharAnim_Template), 00)]
    [EnumExtensions]
    public enum CharAnimIdle : byte { Idle }

    [EnumMembersForTemplate(typeof(CharAnim_Template), 10)]
    [EnumExtensions]
    public enum CharAnimSpawn : byte { Spawn }

    [EnumMembersForTemplate(typeof(CharAnim_Template), 20)]
    [EnumExtensions]
    public enum CharAnimDie : byte { Dying, Dead }

    [EnumMembersForTemplate(typeof(CharAnim_Template), 30)]
    [EnumExtensions]
    public enum CharAnimRun : byte { Run, Sprint }

    [EnumMembersForTemplate(typeof(CharAnim_Template), 40)]
    [EnumExtensions]
    public enum CharAnimAttack : byte { NormalAttack, SpecialAttack }

    [EnumMembersForTemplate(typeof(CharAnim_Template), 50)]
    [EnumExtensions]
    public enum CharAnimHit : byte { Hit }
}