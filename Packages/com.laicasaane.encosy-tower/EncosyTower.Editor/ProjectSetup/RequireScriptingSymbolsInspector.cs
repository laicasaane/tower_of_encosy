#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.ProjectSetup
{
    [CustomEditor(typeof(RequiredScriptingSymbols), true)]
    internal sealed class RequireScriptingSymbolsInspector : UnityEditor.Editor
    {
        public static readonly string UssClassName = "require-scripting-symbols-inspector";
        public static readonly string SymbolsClassName = "symbols";
        public static readonly string SymbolClassName = "symbol";
        public static readonly string SymbolLabelClassName = "symbol__label";
        public static readonly string SymbolTextClassName = "symbol__text";

        [SerializeField] private StyleSheet _styleSheet;

        private SerializedProperty _symbolsProp;

        private void OnEnable()
        {
            _symbolsProp = serializedObject.FindProperty(nameof(RequiredScriptingSymbols._symbols));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.WithStyleSheet(_styleSheet);
            root.AddToClassList(UssClassName);

            var list = new ListView {
                reorderable = true,
                showAddRemoveFooter = true,
                showFoldoutHeader = false,
                showBorder = true,
                horizontalScrollingEnabled = false,
                showBoundCollectionSize = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                selectionType = SelectionType.Single,
                makeItem = MakeItem,
                bindItem = (root, index) => BindItem(root, index, _symbolsProp),
#if UNITY_6000_0_OR_NEWER
                allowAdd = true,
                allowRemove = true,
#endif
            };

            list.WithBindProperty(_symbolsProp);
            root.Add(list.WithClass(SymbolsClassName));

            return root;
        }

        private static VisualElement MakeItem()
            => new VisualElement().WithClass(SymbolClassName)
                .WithChild(new Label().WithClass(SymbolLabelClassName))
                .WithChild(new TextField().WithClass(SymbolTextClassName));

        private static void BindItem(VisualElement root, int index, SerializedProperty symbolsProp)
        {
            var label = root.Q<Label>(className: SymbolLabelClassName);
            label.text = index.ToString();

            var textField = root.Q<TextField>(className: SymbolTextClassName);
            textField.WithBindProperty(symbolsProp.GetArrayElementAtIndex(index));
        }
    }
}

#endif
