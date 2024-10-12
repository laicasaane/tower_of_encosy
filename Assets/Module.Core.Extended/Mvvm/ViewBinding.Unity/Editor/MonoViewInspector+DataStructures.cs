#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Module.Core.TypeWrap;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private readonly record struct EventData(
              EventType Type
            , KeyCode Key
            , EventModifiers Mods
            , int Button
            , Vector2 MousePos
        )
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator EventData(Event ev)
                => new(ev.type, ev.keyCode, ev.modifiers, ev.button, ev.mousePosition);
        }

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
            public PropertyRef() { }

            public PropertyRef(SerializedProperty prop, MonoViewInspector inspector)
            {
                Prop = prop;
                Inspector = inspector;
            }

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
            public ArrayPropertyRef() { }

            public ArrayPropertyRef(SerializedArrayProperty prop, MonoViewInspector inspector)
            {
                Prop = prop;
                Inspector = inspector;
            }

            public SerializedArrayProperty Prop { get; set; }

            public MonoViewInspector Inspector { get; set; }

            public void Reset()
            {
                Prop = null;
                Inspector = null;
            }
        }

        private sealed partial record class MenuItemContext(
              Type ContextType
            , Type InspectorType
            , PropertyRef Instance
        );

        private sealed partial record class MenuItemBinder(
              Type BinderType
            , Type TargetType
            , PropertyRef Instance
        );

        private sealed partial record class MenuItemBinding(
              Type BindingType
            , Type TargetType
            , PropertyRef Instance
        );
    }
}

#endif
