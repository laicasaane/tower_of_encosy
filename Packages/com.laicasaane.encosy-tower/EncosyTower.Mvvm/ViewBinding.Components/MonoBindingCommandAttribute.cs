using System;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
    /// <summary>
    /// Opts-in a <see cref="MonoBindingCommand{T}"/> binding for a <c>UnityEvent</c>-derived
    /// event or C# delegate that is not auto-discovered by <see cref="MonoBinderGenerator"/>,
    /// or overrides how an already-discovered event is bound (e.g. to supply a
    /// <see cref="WrapperType"/> for multi-parameter events or a custom <see cref="Label"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generator auto-discovers public, non-static fields and events whose type derives
    /// from <c>UnityEventBase</c> or is a C# delegate, but it <b>skips events with two or
    /// more type arguments</b> unless this attribute is applied with a <see cref="WrapperType"/>.
    /// Use this attribute when:
    /// <list type="bullet">
    ///   <item>The event field is not public (e.g. <c>internal</c>).</item>
    ///   <item>The event carries two or more parameters and a wrapper value type is
    ///         needed to collapse them into a single argument.</item>
    ///   <item>The event was excluded via <see cref="MonoBindingExcludeAttribute"/> but
    ///         still needs a binding with custom configuration.</item>
    ///   <item>A custom Inspector label is needed (use <see cref="Label"/>).</item>
    /// </list>
    /// </para>
    /// <para>
    /// Must be applied on the same class that carries <see cref="MonoBinderAttribute"/>
    /// (diagnostic <c>MONO_BINDER_0002</c> is raised otherwise). When
    /// <c>[MonoBinder(ExcludeObsolete = true)]</c> is set, applying this attribute to an
    /// obsolete member is an error (diagnostic <c>MONO_BINDER_0006</c>).
    /// Multiple attributes may be applied for multiple bindings.
    /// </para>
    /// <para>
    /// The generator emits a sealed nested class that inherits
    /// <c>MonoBindingCommand&lt;TComponent&gt;</c>. When <see cref="WrapperType"/> is set,
    /// the generated constructor stores a lambda that packs all event arguments into the
    /// wrapper: <c>(a, b, c) =&gt; Callback(new WrapperType(a, b, c))</c>.
    /// </para>
    /// <example>
    /// Bind <c>TMP_InputField.onTextSelection</c>, whose signature is
    /// <c>UnityAction&lt;string, int, int&gt;</c>, using a wrapper struct that collapses
    /// the three arguments into one:
    /// <code>
    /// // Wrapper struct — constructor must accept arguments in event-parameter order.
    /// public readonly struct TMP_TextSelectionData
    /// {
    ///     public readonly string Text;
    ///     public readonly int StringPosition;
    ///     public readonly int StringSelectPosition;
    ///
    ///     public TMP_TextSelectionData(string text, int stringPosition, int stringSelectPosition)
    ///     {
    ///         Text                = text;
    ///         StringPosition      = stringPosition;
    ///         StringSelectPosition = stringSelectPosition;
    ///     }
    /// }
    ///
    /// [MonoBinder(typeof(TMPro.TMP_InputField))]
    /// [MonoBindingCommand(
    ///     nameof(TMPro.TMP_InputField.onTextSelection),
    ///     WrapperType = typeof(TMP_TextSelectionData),
    ///     Label = "On Text Selection")]
    /// public partial class TMP_InputFieldBinder { }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MonoBindingCommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the <c>UnityEvent</c> or delegate field/event on the target component
        /// type to bind.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Optional value type that collapses a multi-parameter <c>UnityEvent</c> into a
        /// single argument for the generated command method.
        /// Required when the event has two or more type arguments, because such events are
        /// skipped by auto-discovery.
        /// The wrapper type must expose a constructor whose parameter order and types match
        /// the event's type arguments exactly — for example, a
        /// <c>UnityAction&lt;string, int, int&gt;</c> event requires a constructor
        /// <c>WrapperType(string, int, int)</c>.
        /// The generator then emits: <c>(a, b, c) =&gt; Callback(new WrapperType(a, b, c))</c>.
        /// </summary>
        public Type WrapperType { get; set; }

        /// <summary>
        /// Optional override for the Inspector label shown in the Unity Editor.
        /// When not set, the label is derived automatically by splitting the member name
        /// on camel-case boundaries (e.g. <c>"onValueChanged"</c> → <c>"On Value Changed"</c>).
        /// </summary>
        public string Label { get; set; }

        public MonoBindingCommandAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
