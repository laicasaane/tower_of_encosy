using System;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    public struct CommandBindingSpec : IEquatable<CommandBindingSpec>
    {
        public string memberName;
        public string memberPascalName;
        public EquatableArray<string> actionTypeArgs;
        public string wrapperTypeName;
        public string label;
        public string callbackMethodName;
        public string generatedClassName;
        public bool skipGeneration;
        public bool isObsolete;
        public string obsoleteMessage;
        public bool isUnityEvent;
        public string delegateFullTypeName;

        public readonly bool Equals(CommandBindingSpec other)
            => string.Equals(memberName, other.memberName, StringComparison.Ordinal)
            && actionTypeArgs.Equals(other.actionTypeArgs)
            && string.Equals(wrapperTypeName, other.wrapperTypeName, StringComparison.Ordinal)
            && string.Equals(label, other.label, StringComparison.Ordinal)
            && string.Equals(callbackMethodName, other.callbackMethodName, StringComparison.Ordinal)
            && skipGeneration == other.skipGeneration
            && isObsolete == other.isObsolete
            && isUnityEvent == other.isUnityEvent
            && string.Equals(delegateFullTypeName, other.delegateFullTypeName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is CommandBindingSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  memberName
                , actionTypeArgs
                , wrapperTypeName
                , label
                , callbackMethodName
                , skipGeneration
                , isObsolete
            )
            .Add(isUnityEvent)
            .Add(delegateFullTypeName);
    }
}
