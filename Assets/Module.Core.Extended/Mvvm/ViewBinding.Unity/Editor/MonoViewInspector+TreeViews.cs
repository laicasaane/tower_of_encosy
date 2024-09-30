#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private sealed class TargetGameObjectTreeView : TreeView
        {
            private readonly GameObject _rootGO;
            private readonly GUIContent _content;

            public TargetGameObjectTreeView(TreeViewState state, GameObject rootGO) : base(state)
            {
                _rootGO = rootGO;
                _content = EditorGUIUtility.ObjectContent(rootGO, typeof(GameObject));

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var rootGO = _rootGO;
                var rootTransform = rootGO.transform;
                var childCount = rootTransform.childCount;
                var content = _content;
                var icon = content.image as Texture2D;
                var treeRoot = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                var allItems = new List<TreeViewItem>(childCount + 1) {
                    new() { id = rootGO.GetInstanceID(), depth = 0, displayName = rootGO.name, icon = icon },
                };

                if (childCount > 0)
                {
                    Build(rootTransform, allItems, 1, icon);
                }

                // Utility method that initializes the TreeViewItem.children and .parent for all items.
                SetupParentsAndChildrenFromDepths(treeRoot, allItems);

                // Return root of the tree
                return treeRoot;
            }

            private static void Build(Transform rootTransform, List<TreeViewItem> list, int depth, Texture2D icon)
            {
                var rootChildCount = rootTransform.childCount;

                for (var i = 0; i < rootChildCount; i++)
                {
                    var child = rootTransform.GetChild(i);
                    var id = child.gameObject.GetInstanceID();
                    list.Add(new TreeViewItem { id = id, depth = depth, displayName = child.name, icon = icon });

                    if (child.childCount > 0)
                    {
                        Build(child, list, depth + 1, icon);
                    }
                }
            }

            public override void OnGUI(Rect rect)
            {
                base.OnGUI(rect);
            }
        }

        private sealed class TargetComponentTreeView : TreeView
        {
            private readonly GameObject _rootGO;
            private readonly Type _componentType;
            private readonly List<Component> _components = new();
            private const string NAME_FORMAT = "{0}";
            private const string NAME_2_FORMAT = "{0} • {1}";
            private const string NAME_INDEX_FORMAT = "{0} • {1}";
            private const string NAME_2_INDEX_FORMAT = "{2} • {0} • {1}";

            public TargetComponentTreeView(TreeViewState state, GameObject rootGO, Type componentType) : base(state)
            {
                _rootGO = rootGO;
                _componentType = componentType;

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var rootGO = _rootGO;
                var componentType = _componentType;

                var rootTransform = rootGO.transform;
                var childCount = rootTransform.childCount;

                var treeRoot = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                var allItems = new List<TreeViewItem>(childCount + 1);
                var components = _components;
                components.Clear();

                rootGO.GetComponents(componentType, components);

                var count = components.Count;

                if (count > 0)
                {
                    var nameFormat = count < 2 ? NAME_FORMAT : NAME_INDEX_FORMAT;
                    var name2Format = count < 2 ? NAME_2_FORMAT : NAME_2_INDEX_FORMAT;

                    for (var k = 0; k < count; k++)
                    {
                        var component = components[k];
                        var type = component.GetType();
                        var content = EditorGUIUtility.ObjectContent(component, type);
                        var icon = content.image as Texture2D;
                        var name = type != componentType
                            ? string.Format(name2Format, rootGO.name, ObjectNames.NicifyVariableName(type.Name), k)
                            : string.Format(nameFormat, rootGO.name, k);

                        allItems.Add(new() { id = component.GetInstanceID(), depth = 0, displayName = name, icon = icon });
                    }
                }

                if (childCount > 0)
                {
                    Build(rootTransform, componentType, allItems, 1, components);
                }

                // Utility method that initializes the TreeViewItem.children and .parent for all items.
                SetupParentsAndChildrenFromDepths(treeRoot, allItems);

                // Return root of the tree
                return treeRoot;
            }

            private static void Build(
                  Transform rootTransform
                , Type componentType
                , List<TreeViewItem> list
                , int depth
                , List<Component> components
            )
            {
                var rootChildCount = rootTransform.childCount;

                for (var i = 0; i < rootChildCount; i++)
                {
                    components.Clear();

                    var child = rootTransform.GetChild(i);
                    child.GetComponents(componentType, components);

                    var count = components.Count;

                    if (count > 0)
                    {
                        var nameFormat = count < 2 ? NAME_FORMAT : NAME_INDEX_FORMAT;
                        var name2Format = count < 2 ? NAME_2_FORMAT : NAME_2_INDEX_FORMAT;

                        for (var k = 0; k < count; k++)
                        {
                            var component = components[k];
                            var type = component.GetType();
                            var id = component.GetInstanceID();
                            var content = EditorGUIUtility.ObjectContent(component, type);
                            var icon = content.image as Texture2D;
                            var name = type != componentType
                                ? string.Format(name2Format, child.name, ObjectNames.NicifyVariableName(type.Name), k)
                                : string.Format(nameFormat, child.name, k);

                            list.Add(new TreeViewItem { id = id, depth = depth, displayName = name, icon = icon });
                        }
                    }

                    if (child.childCount > 0)
                    {
                        Build(child, componentType, list, depth + 1, components);
                    }
                }
            }
        }
    }
}

#endif
