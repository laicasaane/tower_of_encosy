#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Debugging;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Mvvm.ViewBinding.Unity;
using UnityEditor;

namespace EncosyTower.Editor.Mvvm.ViewBinding.Unity
{
    public abstract class BindingContextInspector
    {
        public Type ContextType { get; set; }

        public abstract void OnEnable(
              MonoView view
            , SerializedObject serializedObject
            , SerializedProperty serializedProperty
        );

        public virtual void OnDestroy() { }

        public abstract void OnInspectorGUI();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BindingContextInspectorAttribute : Attribute
    {
        public Type ContextType { get; }

        public BindingContextInspectorAttribute([NotNull] Type contextType)
        {
            Checks.IsTrue(
                  typeof(IBindingContext).IsAssignableFrom(contextType)
                , $"contextType must implement {nameof(IBindingContext)}"
            );

            ContextType = contextType;
        }
    }
}

#endif
