using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statOwnerCountText;
        [SerializeField] private TMP_InputField _statAmountInput;
        [SerializeField] private TMP_Text _affectedPercentText;
        [SerializeField] private TMP_Text _affectedAmountText;
        [SerializeField] private string _affectedPercentFormat;
        [SerializeField] private int _statAmount = 100;

        private float _affectedPercent;

        private void Awake()
        {
            _statAmountInput.text = _statAmount.ToString();

            HudEvents.OnUpdateStatOwnerCounter += OnUpdateStatOwnerCounter;
        }

        public void StatAmount_OnValueChanged(string text)
        {
            if (int.TryParse(text, out var result))
            {
                _statAmount = result;
                UpdateAffectedAmountText();
            }
        }

        public void AffectedAmount_OnValueChanged(float value)
        {
            _affectedPercent = value;
            _affectedPercentText.text = string.Format(_affectedPercentFormat, value * 100f);
            UpdateAffectedAmountText();
        }

        public void SpawnButton_OnClick()
        {
            HudEvents.Spawn(new(_statAmount, _affectedPercent));
        }

        private void UpdateAffectedAmountText()
        {
            var affectedAmount = (int)(_statAmount * _affectedPercent);
            _affectedAmountText.text = affectedAmount.ToString();
        }

        private void OnUpdateStatOwnerCounter(int value)
        {
            if (_statOwnerCountText.IsInvalid())
            {
                return;
            }

            _statOwnerCountText.text = Mathf.Max(0, value).ToString();
        }
    }
}
