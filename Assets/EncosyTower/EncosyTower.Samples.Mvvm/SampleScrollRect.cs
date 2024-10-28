using TMPro;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    public sealed class SampleScrollRect : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private TMP_Text _prefab;
        [SerializeField] private ushort _count;

        private void Awake()
        {
            var content = _content;
            var prefab = _prefab;
            var count = (int)_count;

            for (var i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab, content, false);
                instance.gameObject.name = instance.text = $"text_{i}";
            }

            prefab.gameObject.SetActive(false);
        }
    }
}
