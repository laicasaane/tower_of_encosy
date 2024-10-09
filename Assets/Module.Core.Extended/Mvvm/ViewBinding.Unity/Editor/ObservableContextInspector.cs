#if UNITY_EDITOR

using System;
using System.Diagnostics.CodeAnalysis;
using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using UnityEditor;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    public abstract class ObservableContextInspector
    {
        public Type ContextType { get; set; }

        public abstract void OnEnable(
              UnityEngine.Object target
            , SerializedObject serializedObject
            , SerializedProperty serializedProperty
        );

        public abstract void OnDestroy();

        public abstract void OnInspectorGUI();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ObservableContextInspectorAttribute : Attribute
    {
        public Type ContextType { get; }

        public ObservableContextInspectorAttribute([NotNull] Type contextType)
        {
            Checks.IsTrue(typeof(ObservableContext).IsAssignableFrom(contextType), "contextType must be a subclass of ObservableContext");
            ContextType = contextType;
        }
    }

    [ObservableContextInspector(typeof(ObservableUnityObjectContext))]
    public sealed class ObservableUnityObjectContextInspector : ObservableContextInspector
    {
        public override void OnEnable(
              UnityEngine.Object target
            , SerializedObject serializedObject
            , SerializedProperty serializedProperty
        )
        {

        }

        public override void OnDestroy()
        {
        }

        public override void OnInspectorGUI()
        {
        }
    }
}

#endif
