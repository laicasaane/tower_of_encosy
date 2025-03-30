using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.VisualCommands
{
    [RequireComponent(typeof(UIDocument))]
    public partial class VisualCommanderPage : MonoBehaviour
    {
        [SerializeField] private float _directoryListWidth = 200f;
        [SerializeField] private bool _horizontal = true;

        private UIDocument _document;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void Start()
        {
            var view = VisualCommanderAPI.CreateView(
                  _document.rootVisualElement
                , _directoryListWidth
                , _horizontal
            );

            view.OnClose += OnClose;
        }

        private void OnClose()
        {
            gameObject.SetActive(false);
        }
    }
}
