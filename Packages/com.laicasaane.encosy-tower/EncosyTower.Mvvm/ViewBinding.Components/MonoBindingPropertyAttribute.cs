using System;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
    /// <summary>
    /// Opts-in a <see cref="MonoBindingProperty{T}"/> binding for a member that is not
    /// auto-discovered by <see cref="MonoBinderGenerator"/>, or overrides how an
    /// already-discovered property is bound (e.g. to route assignment through a
    /// notification-suppressing setter method).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The generator auto-discovers public, non-static properties that have a public setter
    /// and whose type is <b>not</b> a <c>UnityEvent</c> or delegate. Use this attribute when:
    /// <list type="bullet">
    ///   <item>The target property or field is not public (e.g. <c>internal</c>).</item>
    ///   <item>The property was excluded via <see cref="MonoBindingExcludeAttribute"/> but
    ///         still needs a binding with custom configuration.</item>
    ///   <item>Assignment must go through a method instead of a direct setter
    ///         (e.g. <see cref="SetterMethod"/> = <c>nameof(Slider.SetValueWithoutNotify)</c>).</item>
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
    /// <c>MonoBindingProperty&lt;TComponent&gt;</c> with a <c>[BindingProperty]</c>-annotated
    /// private setter method. When <see cref="SetterMethod"/> is set the method body calls
    /// <c>targets[i].SetterMethod(value)</c>; otherwise it assigns
    /// <c>targets[i].MemberName = value</c>.
    /// </para>
    /// <example>
    /// Bind <c>Slider.value</c> through <c>SetValueWithoutNotify</c> to prevent the slider
    /// from firing its <c>onValueChanged</c> event when the ViewModel updates the view:
    /// <code>
    /// [MonoBinder(typeof(UnityEngine.UI.Slider))]
    /// [MonoBindingProperty(
    ///     nameof(UnityEngine.UI.Slider.value),
    ///     SetterMethod = nameof(UnityEngine.UI.Slider.SetValueWithoutNotify),
    ///     Label = "Value (Silent)")]
    /// public partial class UnityUISliderBinder { }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MonoBindingPropertyAttribute : Attribute
    {
        /// <summary>
        /// The name of the property or field on the target component type to bind.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// When <see langword="true"/>, the generator delegates value assignment to a
        /// <c>partial void</c> method on the outer binder class, instead of
        /// assigning the property directly or calling a component method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The generator emits the following stub on the outer binder class and the inner
        /// binding loop body calls it:
        /// <code>
        /// private static partial void Set_X(TComponent target, TValue value);
        /// </code>
        /// The method name is derived from <see cref="MemberName"/>:
        /// <list type="bullet">
        ///   <item>If the name starts with <c>Set</c> followed by an uppercase letter, the
        ///         <c>Set</c> prefix is stripped and replaced with <c>Set_</c>
        ///         (e.g. <c>SetActive</c> → <c>Set_Active</c>).</item>
        ///   <item>Otherwise <c>Set_</c> is prepended and the first character is uppercased
        ///         (e.g. <c>active</c> → <c>Set_Active</c>).</item>
        /// </list>
        /// The user must provide the partial method implementation on the outer binder class.
        /// </para>
        /// <example>
        /// <code>
        /// [MonoBinder(typeof(UnityEngine.GameObject))]
        /// [MonoBindingProperty(nameof(UnityEngine.GameObject.SetActive), UseCustomSetter = true)]
        /// public partial class GameObjectBinder
        /// {
        ///     private static partial void Set_Active(GameObject target, bool value)
        ///     {
        ///         target.SetActive(value);
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public bool UseCustomSetter { get; set; }

        /// <summary>
        /// Optional override for the Inspector label shown in the Unity Editor.
        /// When not set, the label is derived automatically by splitting the member name
        /// on camel-case boundaries (e.g. <c>"minValue"</c> → <c>"Min Value"</c>).
        /// </summary>
        public string Label { get; set; }

        public MonoBindingPropertyAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
