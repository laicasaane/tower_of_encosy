#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Modules.TypeWrap;
using UnityEditor;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        [WrapRecord]
        private readonly partial record struct DestType(Type _);

        [WrapRecord]
        private readonly partial record struct SourceType(Type _);

        private readonly partial record struct BinderDefinition(
              Type BinderType
            , Type TargetType
            , string Label
            , string Directory
        )
        {
            public readonly bool IsValid => BinderType != null && TargetType != null;
        }

        private sealed class MemberMap : Dictionary<string, Type> { }

        private sealed class PropertyRef
        {
            public SerializedProperty Prop { get; set; }

            public MonoViewInspector Inspector { get; set; }

            public void Reset()
            {
                Prop = null;
                Inspector = null;
            }
        }

        private sealed class ArrayPropertyRef
        {
            public ArrayPropertyRef(SerializedArrayProperty prop, MonoViewInspector inspector)
            {
                Prop = prop;
                Inspector = inspector;
            }

            public SerializedArrayProperty Prop { get; private set; }

            public MonoViewInspector Inspector { get; private set; }

            public void Reset()
            {
                Prop = null;
                Inspector = null;
            }
        }

        private sealed partial record MenuItemContext(
              Type ContextType
            , Type InspectorType
            , PropertyRef Instance
        );

        private sealed partial record MenuItemBinder(
              Type BinderType
            , Type TargetType
            , PropertyRef Instance
        );

        private sealed partial record MenuItemBinding(
              Type BindingType
            , Type TargetType
            , PropertyRef Instance
        );
    }
}

#endif
