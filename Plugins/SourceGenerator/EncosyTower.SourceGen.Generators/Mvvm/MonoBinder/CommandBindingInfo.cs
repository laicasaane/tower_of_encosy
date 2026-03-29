using System;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    public struct CommandBindingInfo : IEquatable<CommandBindingInfo>
    {
        public string memberName;               // "onClick"
        public string memberPascalName;         // "OnClick"
        public EquatableArray<string> actionTypeArgs; // [] or ["global::System.Single"]
        public string wrapperTypeName;          // "" or full wrapper type
        public string label;                    // "On Click"
        public string callbackMethodName;       // "OnClick"
        public string generatedClassName;       // "UnityUIButtonBindingOnClick"
        public bool skipGeneration;
        public bool isObsolete;
        public string obsoleteMessage;

        /// <summary><see langword="true"/> when the backing event is a <c>UnityEvent</c>/<c>UnityEvent&lt;T&gt;</c>;
        /// <see langword="false"/> when it is a plain C# delegate/event.</summary>
        public bool isUnityEvent;

        /// <summary>Full type name of the C# delegate type (e.g. <c>global::System.EventHandler&lt;Foo&gt;</c>).
        /// Empty when <see cref="isUnityEvent"/> is <see langword="true"/>.</summary>
        public string delegateFullTypeName;

        public readonly bool Equals(CommandBindingInfo other)
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
            => obj is CommandBindingInfo other && Equals(other);

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
