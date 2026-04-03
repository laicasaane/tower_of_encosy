using TMPro;
using UnityEngine;

namespace EncosyTower.Samples.UserDataVault.SimpleUsage
{
    internal class ItemCurrencyController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_InputField _input;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        public int Value
        {
            get
            {
                int.TryParse(_input.text, out var result);
                return result;
            }

            set
            {
                _input.text = value.ToString();
            }
        }
    }
}
