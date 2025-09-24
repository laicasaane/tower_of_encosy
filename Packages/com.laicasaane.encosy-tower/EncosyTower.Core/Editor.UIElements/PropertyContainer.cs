// https://github.com/OscarAbraham/UITKEditorAid/blob/fe298422c5359d3acc69e6779c4d4f75c2a2ffc1/Editor/PropertyContainer.cs

// MIT License
//
// Copyright (c) 2021 OscarAbraham
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if UNITY_EDITOR

using System;
using EncosyTower.Annotations;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.UIElements
{
    /// <summary>
    /// Element that shows a <see cref="SerializedProperty">SerializedProperty's</see> prefab override indicators,
    /// and the property's menu on context click. <c>UXML support</c>
    /// </summary>
    /// <remarks>
    /// Currently, Unity only adds these capabilities for fields that inherit
    /// from <see cref="BaseField{TValueType}"/> or <see cref="Foldout"/>.
    /// This element makes those features available everywhere.
    /// It can be used with any SerializedProperty, even those that have nested properties inside.
    /// To use it, assign a <see cref="bindingPath"/> to the property it represents, and call the
    /// <see cref="VisualElement.Add(VisualElement)"/> method to add the UI for that property.
    /// <para>
    /// It should also show other kinds of indicators, like the yellow ones from the localization package,
    /// but I haven't tested that.
    /// </para>
    /// </remarks>
    /// <example>
    /// The <see cref="PrefabOverrideUssClassName"/> can be used to apply custom styles when theres a prefab override.
    /// For example, a custom style sheet could do something like this to make a particular label bold
    /// when there's an override:
    /// <code language="css"><![CDATA[
    /// .editor-aid-property-container--prefab-override .my-custom-label-class {
    ///     -unity-font-style: bold;
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class PropertyContainer : VisualElement, IHasBindingPath
    {
        /// <summary> USS class name of elements of this type. </summary>
        public static readonly string UssClassName = "editor-aid-property-container";

        /// <summary> USS class name of elements of this type when their property is a prefab override. </summary>
        public static readonly string PrefabOverrideUssClassName = $"{UssClassName}--prefab-override";

        /// <summary> USS class name for the content element. </summary>
        public static readonly string ContentUssClassName = $"{UssClassName}__content";

        /// <summary> USS class name for an invisible element that makes Unity apply the relevant SerializedProperty features. </summary>
        public static readonly string PropertyProxyUssClassName = $"{UssClassName}__property-proxy";

        private const long CHECK_PREFAB_OVERRIDE_INTERVAL = 500;

        /// <summary>
        /// An event that's triggered when a change in the property's override status is detected.
        /// Receives a <see cref="bool"/> that indicates whether the property is a prefab override.
        /// </summary>
        public event Action<bool> OnPrefabOverrideChanged;

        private readonly Foldout _propertyProxy;
        private readonly Toggle _proxyToggle;
        private readonly VisualElement _contentContainer;

        private IVisualElementScheduledItem _checkPrefabOverrideScheduled;
        private bool _hasPropertyOverride = false;

        /// <summary> Constructor. </summary>
        public PropertyContainer()
            : this(string.Empty)
        { }

        /// <summary>
        /// Constructor. The Property parameter just sets the <see cref="bindingPath"/>; it still needs to be bound.
        /// </summary>
        /// <param name="property"> The property represented by this element. </param>
        public PropertyContainer(SerializedProperty property)
            : this(property?.propertyPath)
        { }

        /// <summary>
        /// Constructor. The Property parameter just sets the <see cref="bindingPath"/>; it still needs to be bound.
        /// </summary>
        /// <param name="property"> The property represented by this element.</param>
        /// <param name="checkForPrefabOverride">
        /// Whether to trigger <see cref="OnPrefabOverrideChanged"/> and apply a custom USS class
        /// when a prefab override is detected.
        /// </param>
        public PropertyContainer(SerializedProperty property, bool checkForPrefabOverride)
            : this(property?.propertyPath, checkForPrefabOverride)
        { }

        /// <summary>
        /// Constructor. Receives a string that is assigned to <see cref="bindingPath"/>.
        /// </summary>
        /// <param name="propertyPath"> The path of the property represented by this element. </param>
        public PropertyContainer(string propertyPath)
            : this(propertyPath, true)
        { }

        /// <summary>
        /// Constructor. Receives a string that is assigned to <see cref="bindingPath"/>.
        /// </summary>
        /// <param name="propertyPath">The path of the property represented by this element.</param>
        /// <param name="checkForPrefabOverride">
        /// Whether to trigger <see cref="OnPrefabOverrideChanged"/> and apply a custom USS class
        /// when a prefab override is detected.
        /// </param>
        public PropertyContainer(string propertyPath, bool checkForPrefabOverride)
        {
            AddToClassList(UssClassName);

            // We use a separate content container to be able to put a proxy element on top in case it's needed.
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList(ContentUssClassName);
            _contentContainer.pickingMode = PickingMode.Position;
            hierarchy.Add(_contentContainer);

            _propertyProxy = new Foldout { pickingMode = PickingMode.Ignore };
            _propertyProxy.AddToClassList(PropertyProxyUssClassName);
            _propertyProxy.style.position = Position.Absolute;
            _propertyProxy.style.top
                = _propertyProxy.style.bottom
                = _propertyProxy.style.left
                = _propertyProxy.style.right = 0;

            _propertyProxy.style.opacity = 0;

            _proxyToggle = _propertyProxy.Q<Toggle>(null, Foldout.toggleUssClassName);
            _proxyToggle.focusable = false;
            _proxyToggle.pickingMode = PickingMode.Ignore;
            _proxyToggle.Query().ForEach(ve => ve.pickingMode = PickingMode.Ignore);
            _proxyToggle.style.position = Position.Absolute;
            _proxyToggle.style.top = _proxyToggle.style.bottom = _proxyToggle.style.left = 0;
            _proxyToggle.style.width = 0;
            _proxyToggle.style.marginBottom
                = _proxyToggle.style.marginTop
                = _proxyToggle.style.marginLeft
                = _proxyToggle.style.marginRight = 0;

            hierarchy.Add(_propertyProxy);
            bindingPath = propertyPath;

            RegisterCallback<PointerUpEvent>(OnPointerUp);
            CheckForPrefabOverride = checkForPrefabOverride;
        }

        /// <summary>
        ///  Whether to trigger <see cref="OnPrefabOverrideChanged"/> and apply a custom USS class when a prefab
        ///  override is detected. True by default. Consider setting it false if you're not using those features.
        ///  This check isn't very expensive, but it can add up when there are many PropertyContainers.
        ///  The element will still show the overrides blue bar and the prefab overrides menu when this is false.
        /// </summary>
        [UxmlAttribute]
        public bool CheckForPrefabOverride
        {
            get => _checkPrefabOverrideScheduled?.isActive ?? false;
            set
            {
                if (value)
                {
                    _checkPrefabOverrideScheduled ??= schedule.Execute(CheckPrefabOverride)
                        .Every(CHECK_PREFAB_OVERRIDE_INTERVAL);

                    _checkPrefabOverrideScheduled.Resume();
                }
                else
                {
                    _checkPrefabOverrideScheduled?.Pause();
                }
            }
        }

        /// <summary> The path to property represented by this element. </summary>
        [UxmlAttribute]
        public string bindingPath
        {
            get => _propertyProxy.bindingPath;
            set => _propertyProxy.bindingPath = value;
        }

        [RemoveFromDocs]
        public override VisualElement contentContainer => _contentContainer;

        private void OnPointerUp(PointerUpEvent e)
        {
            // Prevent getting stuck in an infinite loop with the fake events sent to the proxyToggle.
            if (e.target == _proxyToggle)
                return;

            // Unity's current implementation doesn't detect ctrl + click on macOS,
            // but that doesn't stop us from supporting it.
            if (e.button == 1 || (Application.platform == RuntimePlatform.OSXEditor && e.button == 0 && e.ctrlKey))
            {
                if (e.target is not VisualElement)
                {
                    return;
                }

                e.StopPropagation();

                // In 2021.3, the menu is displayed from the toggleElement's position, so we put it under the mouse.
                // We need to remove this code in newer versions because they display prefab blue bars next to the
                // Toggle instead of next to the whole Foldout.

                var fakeSystemEvent = new Event() {
                    button = 1,
                    mousePosition = e.position,
                    type = EventType.MouseDown,
                };

                using (var fakeDownEvent = PointerDownEvent.GetPooled(fakeSystemEvent))
                {
                    fakeDownEvent.target = _proxyToggle;
                    _proxyToggle.SendEvent(fakeDownEvent);
                }

                fakeSystemEvent.type = EventType.MouseUp;

                using (var fakeUpEvent = PointerUpEvent.GetPooled(fakeSystemEvent))
                {
                    fakeUpEvent.target = _proxyToggle;
                    _proxyToggle.SendEvent(fakeUpEvent);
                }
            }
        }

        private void CheckPrefabOverride()
        {
            bool proxyHasPropertyOverride = _proxyToggle.ClassListContains(
                BindingExtensions.prefabOverrideUssClassName
            );

            if (proxyHasPropertyOverride != _hasPropertyOverride)
            {
                _hasPropertyOverride = proxyHasPropertyOverride;
                EnableInClassList(PrefabOverrideUssClassName, proxyHasPropertyOverride);
                OnPrefabOverrideChanged?.Invoke(proxyHasPropertyOverride);
            }
        }
    }
}

#endif
