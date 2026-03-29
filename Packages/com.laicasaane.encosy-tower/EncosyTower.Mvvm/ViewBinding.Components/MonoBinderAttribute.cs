using System;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
    /// <summary>
    /// Entry-point decorator that designates a <c>partial</c> class as a MonoBinder and
    /// triggers <see cref="MonoBinderGenerator"/> to generate nested
    /// <c>MonoBindingProperty&lt;T&gt;</c> and <c>MonoBindingCommand&lt;T&gt;</c> binding
    /// classes for the specified <see cref="UnityEngine.Object"/> component type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generator walks the component type's inheritance chain (stopping before
    /// <c>UnityEngine.Object</c>) and auto-discovers:
    /// <list type="bullet">
    ///   <item>Public, non-static <b>properties</b> that have a public setter and are not
    ///         a <c>UnityEvent</c>/<c>delegate</c> type — one <c>MonoBindingProperty&lt;T&gt;</c>
    ///         nested class is emitted per property.</item>
    ///   <item>Public, non-static <b>fields and events</b> whose type derives from
    ///         <c>UnityEventBase</c> or is a C# delegate — one <c>MonoBindingCommand&lt;T&gt;</c>
    ///         nested class is emitted per event. Multi-parameter events (two or more type
    ///         arguments) are skipped during auto-discovery unless a
    ///         <see cref="MonoBindingCommandAttribute"/> with <c>WrapperType</c> is provided.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Use <see cref="MonoBindingPropertyAttribute"/> or <see cref="MonoBindingCommandAttribute"/>
    /// to opt-in members that are not auto-discovered (e.g. non-public members, members that
    /// require a custom setter call, or multi-parameter events with a wrapper type).
    /// Use <see cref="MonoBindingExcludeAttribute"/> to opt-out individual members, and
    /// <see cref="MonoBinderExcludeParentAttribute"/> to stop the discovery walk at a
    /// specific base type.
    /// </para>
    /// <para>
    /// Only one <c>[MonoBinder]</c> is allowed per class. The decorated class must be
    /// <c>partial</c>. The component type must inherit from <c>UnityEngine.Object</c>
    /// (diagnostic <c>MONO_BINDER_0001</c> is raised otherwise).
    /// </para>
    /// <example>
    /// A binder for <c>UnityEngine.UI.Button</c> that wraps all generated code in a
    /// conditional compilation block and skips obsolete members:
    /// <code>
    /// [MonoBinder(typeof(UnityEngine.UI.Button),
    ///     PreprocessorGuard = "UNITY_UGUI",
    ///     ExcludeObsolete   = true)]
    /// [MonoBinderExcludeParent(typeof(UnityEngine.UI.Selectable))]
    /// public partial class UnityUIButtonBinder { }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MonoBinderAttribute : Attribute
    {
        /// <summary>
        /// The <c>UnityEngine.Object</c> subtype whose public members are reflected to
        /// generate binding classes. Must inherit from <c>UnityEngine.Object</c>
        /// (diagnostic <c>MONO_BINDER_0001</c> is raised if it does not).
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// When set, the entire generated source file is wrapped in
        /// <c>#if GUARD … #endif</c>, where <c>GUARD</c> is this value.
        /// Useful for platform- or package-conditional binders
        /// (e.g. <c>"UNITY_TEXTMESHPRO"</c>).
        /// </summary>
        public string PreprocessorGuard { get; set; }

        /// <summary>
        /// When <see langword="true"/>, any member on the component type that is annotated
        /// with <c>[Obsolete]</c> is silently skipped during auto-discovery.
        /// Explicitly opt-ing in an obsolete member via
        /// <see cref="MonoBindingPropertyAttribute"/> or
        /// <see cref="MonoBindingCommandAttribute"/> while this flag is
        /// <see langword="true"/> is an error (diagnostic <c>MONO_BINDER_0006</c>).
        /// </summary>
        public bool ExcludeObsolete { get; set; }

        public MonoBinderAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Stops the member auto-discovery walk at the specified base type: members declared on
    /// <paramref name="parentType"/> and all of its ancestors are excluded from the
    /// generated bindings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must be applied on the same class that carries <see cref="MonoBinderAttribute"/>
    /// (diagnostic <c>MONO_BINDER_0002</c> is raised otherwise).
    /// </para>
    /// <para>
    /// Constraints (both raise errors):
    /// <list type="bullet">
    ///   <item><c>MONO_BINDER_0004</c> — <paramref name="parentType"/> must not be the
    ///         component type itself.</item>
    ///   <item><c>MONO_BINDER_0005</c> — <paramref name="parentType"/> must appear
    ///         somewhere in the component type's inheritance chain.</item>
    /// </list>
    /// </para>
    /// <example>
    /// Exclude <c>Selectable</c> members (navigation, transition, targetGraphic, …)
    /// from a <c>Button</c> binder so only <c>Button</c>-specific members are generated:
    /// <code>
    /// [MonoBinder(typeof(UnityEngine.UI.Button))]
    /// [MonoBinderExcludeParent(typeof(UnityEngine.UI.Selectable))]
    /// public partial class UnityUIButtonBinder { }
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="parentType">
    /// A base class of the component type at which the inheritance walk stops.
    /// </param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MonoBinderExcludeParentAttribute : Attribute
    {
        /// <summary>
        /// The base type at which the generator stops walking the component's inheritance
        /// chain. Members declared on this type and its ancestors are excluded.
        /// </summary>
        public Type ParentType { get; }

        public MonoBinderExcludeParentAttribute(Type parentType)
        {
            ParentType = parentType;
        }
    }

    /// <summary>
    /// Opts-out a single public member from auto-discovery, preventing the generator
    /// from emitting a binding class for it. Can be applied multiple times on the same class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must be applied on the same class that carries <see cref="MonoBinderAttribute"/>
    /// (diagnostic <c>MONO_BINDER_0002</c> is raised otherwise).
    /// </para>
    /// <para>
    /// Diagnostics:
    /// <list type="bullet">
    ///   <item><c>MONO_BINDER_0003</c> (warning) — the member name does not match any
    ///         public non-static property, field, or event on the component type.</item>
    ///   <item><c>MONO_BINDER_0007</c> (info) — the attribute is redundant because
    ///         <c>[MonoBinder(ExcludeObsolete = true)]</c> already suppresses the same
    ///         obsolete member.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This attribute does <b>not</b> prevent an explicit <see cref="MonoBindingPropertyAttribute"/>
    /// or <see cref="MonoBindingCommandAttribute"/> from re-including the same member.
    /// </para>
    /// <example>
    /// Exclude the <c>pixelsPerUnit</c> property from an <c>Image</c> binder:
    /// <code>
    /// [MonoBinder(typeof(UnityEngine.UI.Image))]
    /// [MonoBindingExclude(nameof(UnityEngine.UI.Image.pixelsPerUnit))]
    /// public partial class UnityUIImageBinder { }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MonoBindingExcludeAttribute : Attribute
    {
        /// <summary>
        /// The name of the public non-static property, field, or event on the component type
        /// to exclude from auto-discovery.
        /// </summary>
        public string MemberName { get; }

        public MonoBindingExcludeAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
