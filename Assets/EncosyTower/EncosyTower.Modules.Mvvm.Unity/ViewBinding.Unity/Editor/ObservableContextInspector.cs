#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEditor;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    public abstract class ObservableContextInspector
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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ObservableContextInspectorAttribute : Attribute
    {
        public Type ContextType { get; }

        public ObservableContextInspectorAttribute([NotNull] Type contextType)
        {
            Checks.IsTrue(
                  typeof(ObservableContext).IsAssignableFrom(contextType)
                , "contextType must be a subclass of ObservableContext"
            );

            ContextType = contextType;
        }
    }
}

#endif
