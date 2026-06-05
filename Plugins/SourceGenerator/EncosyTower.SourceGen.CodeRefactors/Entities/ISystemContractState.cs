using System;

namespace EncosyTower.SourceGen.CodeRefactors.Entities
{
    internal struct ISystemContractState
    {
        public bool hasPartial;
        public bool hasISystem;
        public MemberFlags existingMembers;

        public readonly bool AllMemberExists
            => existingMembers == MemberFlags.All;
    }

    [Flags]
    internal enum MemberFlags
    {
        None = 0,
        OnCreate = 1 << 0,
        OnUpdate = 1 << 1,
        OnDestroy = 1 << 2,
        All = OnCreate | OnUpdate | OnDestroy,
    }
}
