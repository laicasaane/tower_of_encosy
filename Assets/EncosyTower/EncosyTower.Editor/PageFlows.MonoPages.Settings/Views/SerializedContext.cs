using System;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.Types;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows.MonoPages.Settings.Views
{
    internal class SerializedContext
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly MonoPageFlowSettings Settings;
        public readonly SerializedObject Object;

        public SerializedContext(ScriptableObject settings, SerializedObject serializedObject)
        {
            Settings = settings as MonoPageFlowSettings;
            Object = serializedObject;
            Type = Settings.GetType();
            Name = Type.GetNameWithoutSuffix("Settings");
        }

        public SerializedProperty GetLoaderStrategyProperty()
            => Object.FindProperty(nameof(MonoPageFlowSettings._loaderStrategy));

        public SerializedProperty GetMessageScopeProperty()
            => Object.FindProperty(nameof(MonoPageFlowSettings._messageScope));

        public SerializedProperty GetLogEnvironmentProperty()
            => Object.FindProperty(nameof(MonoPageFlowSettings._logEnvironment));
    }
}
