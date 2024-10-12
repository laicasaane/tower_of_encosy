#if UNITY_EDITOR

using Module.Core.Extended.Mvvm.ViewBinding.Unity;
using Module.Core.Logging;
using Module.Core.Mvvm.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    [ObservableContextInspector(typeof(ObservableUnityObjectContext))]
    public sealed class ObservableUnityObjectContextInspector : ObservableContextInspector
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

                var obj = EditorGUILayout.ObjectField(_objectProperty.objectReferenceValue, typeof(UnityEngine.Object), true);

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
                          "Observable Unity Object Context"
                        , "Context Object must implement IObservableObject interface."
                        , "I understand"
                    );
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(s_findLabel);

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
    }
}

#endif
