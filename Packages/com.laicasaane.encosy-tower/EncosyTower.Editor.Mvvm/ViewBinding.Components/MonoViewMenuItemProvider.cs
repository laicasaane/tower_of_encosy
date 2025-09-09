#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.ViewBinding.Components;
using EncosyTower.Mvvm.ViewBinding.Contexts;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EncosyTower.Editor.Mvvm
{
    internal static class MonoViewMenuItemProvider
    {
        [MenuItemProvider]
        public static IEnumerable<EditorMenuItem> ProvideMenuItemToAddMonoView()
        {
            const string MENU_NAME = "CONTEXT/{0}/Add MonoView";

            var observableTypes = TypeCache.GetTypesDerivedFrom<IObservableObject>();
            var monoViewCandidateTypes = TypeCache.GetTypesDerivedFrom<MonoView>();
            var typeOfComponent = typeof(Component);
            var items = new List<EditorMenuItem>(observableTypes.Count);
            var monoViewTypes = new List<Type>(monoViewCandidateTypes.Count) {
                typeof(MonoView),
            };

            foreach (var candidate in monoViewCandidateTypes)
            {
                if (candidate.IsAbstract
                    || candidate.ContainsGenericParameters
                )
                {
                    continue;
                }

                monoViewTypes.Add(candidate);
            }

            foreach (var typeOfObservable in observableTypes)
            {
                if (typeOfObservable.IsAbstract
                    || typeOfObservable.ContainsGenericParameters
                    || typeOfComponent.IsAssignableFrom(typeOfObservable) == false
                )
                {
                    continue;
                }

                foreach (var typeOfMonoView in monoViewTypes)
                {
                    items.Add(new EditorMenuItem {
                        Name = string.Format(MENU_NAME, typeOfObservable.Name),
                        Priority = 1000,
                        Execute = () => {
                            var gameObject = Selection.activeGameObject;

                            if (gameObject.IsInvalid())
                            {
                                return;
                            }

                            if (gameObject.TryGetComponent(typeOfObservable, out var observableComponent) == false)
                            {
                                return;
                            }

                            if (gameObject.TryGetComponent(typeOfMonoView, out var existingComponent))
                            {
                                Selection.activeObject = existingComponent;
                                return;
                            }

                            var monoViewComponent = gameObject.AddComponent(typeOfMonoView) as MonoView;

                            MoveComponentUp(typeOfObservable, typeOfMonoView, gameObject, monoViewComponent);

                            monoViewComponent._context = new UnityObjectBindingContext {
                                _object = observableComponent,
                            };
                        },
                    });
                }
            }

            return items;
        }

        private static void MoveComponentUp(
              Type typeOfObservable
            , Type typeOfMonoView
            , GameObject gameObject
            , Component monoViewComponent
        )
        {
            var components = gameObject.GetComponents(typeof(Component));

            for (var i = components.Length - 1; i >= 0; i--)
            {
                var type = components[i].GetType();

                if (type == typeOfMonoView)
                {
                    continue;
                }

                if (type == typeOfObservable)
                {
                    break;
                }

                ComponentUtility.MoveComponentUp(monoViewComponent);
            }
        }
    }
}

#endif
