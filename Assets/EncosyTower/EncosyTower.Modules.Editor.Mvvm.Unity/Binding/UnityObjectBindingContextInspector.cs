#if UNITY_EDITOR

using EncosyTower.Modules.Mvvm.ComponentModel;
using EncosyTower.Modules.Mvvm.ViewBinding.Unity;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
{
    [BindingContextInspector(typeof(UnityObjectBindingContext))]
    public sealed class UnityObjectBindingContextInspector : BindingContextInspector
    {
        private static readonly GUIContent s_findLabel = new("Find Nearest Context");
        private static readonly GUIContent s_childrenLabel = new("On Children");
        private static readonly GUIContent s_parentsLabel = new("On Parents");
        private static readonly GUIContent s_contextLabel = new("Context");

        private MonoView _view;
        private SerializedObject _serializedObject;
        private SerializedProperty _objectProperty;

        public override void OnEnable(
              MonoView view
            , SerializedObject serializedObject
            , SerializedProperty contextProperty
        )
        {
            _view = view;
            _serializedObject = serializedObject;
            _objectProperty = contextProperty.FindPropertyRelative("_object");
        }

        public override void OnInspectorGUI()
        {
            if (_view == false)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(s_contextLabel);

                var obj = EditorGUILayout.ObjectField(
                      _objectProperty.objectReferenceValue
                    , typeof(UnityEngine.Object)
                    , true
                );

                obj = FindContext(obj);

                if (obj == false || obj is IObservableObject)
                {
                    Undo.RecordObject(_view, "Change context object reference");

                    _objectProperty.objectReferenceValue = obj;
                    _serializedObject.ApplyModifiedProperties();
                    _serializedObject.Update();
                }
                else
                {
                    EditorUtility.DisplayDialog(
                          "Unity Object Binding Context"
                        , $"Context Object must implement {nameof(IObservableObject)} interface."
                        , "I understand"
                    );
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(s_findLabel);

                if (GUILayout.Button(s_childrenLabel, EditorStyles.miniButtonLeft))
                {
                    var obj = _view.gameObject.GetComponentInChildren<IObservableObject>();
                    OnFindContext(obj);
                }

                if (GUILayout.Button(s_parentsLabel, EditorStyles.miniButtonRight))
                {
                    var obj = _view.gameObject.GetComponentInParent<IObservableObject>();
                    OnFindContext(obj);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnFindContext(IObservableObject observableObject)
        {
            if (observableObject == null || observableObject is not UnityEngine.Object obj)
            {
                EditorUtility.DisplayDialog(
                      "Find Nearest Context"
                    , "Cannot find any object that implements IObservableObject interface."
                    , "I understand"
                );
                return;
            }

            Undo.RecordObject(_view, "Find nearest context");

            _objectProperty.objectReferenceValue = obj;
            _serializedObject.ApplyModifiedProperties();
            _serializedObject.Update();
        }

        private static UnityEngine.Object FindContext(UnityEngine.Object src)
        {
            if (src is GameObject go && go.TryGetComponent<IObservableObject>(out var result))
            {
                return result as UnityEngine.Object;
            }

            return src;
        }
    }
}

#endif
