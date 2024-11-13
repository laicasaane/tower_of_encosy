using EncosyTower.Modules.Mvvm.ComponentModel;
using EncosyTower.Modules.Mvvm.Input;
using UnityEngine;

namespace EncosyTower.Samples.Mvvm
{
    public sealed partial class SampleMvvm : MonoBehaviour, IObservableObject
    {
        [SerializeField] private float _scrollSpeed = 2f;
        [SerializeField] private float _scrollInterval = 0.05f;
        [SerializeField] private float _scrollStop = 1f;

        private float _scrollDirection = 1f;
        private float _scrollIntervalElapsed;

        [ObservableProperty]
        public float ScrollPosition { get => Get_ScrollPosition(); set => Set_ScrollPosition(value); }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Status))]
        public bool Stopped { get => Get_Stopped(); set => Set_Stopped(value); }

        public string Status => Stopped
            ? "Status: <color=\"red\">Stopped</color>"
            : "Status: <color=\"green\">Playing</color>";

        private void Start()
        {
            ScrollPosition = 1f;
        }

        private void Update()
        {
            if (_stopped)
            {
                return;
            }

            UpdateScroll();
        }

        private void UpdateScroll()
        {
            _scrollIntervalElapsed += Time.deltaTime;

            if (_scrollIntervalElapsed < _scrollInterval)
            {
                return;
            }

            _scrollIntervalElapsed = 0f;

            var scrollPos = ScrollPosition;
            scrollPos += _scrollSpeed * _scrollDirection * Time.deltaTime;

            if (scrollPos is >= 1f or <= 0f)
            {
                _scrollDirection *= -1f;
                _scrollIntervalElapsed = _scrollStop * -1f;
            }

            ScrollPosition = Mathf.Clamp(scrollPos, 0f, 1f);
        }

        [RelayCommand]
        private void OnStop()
        {
            Stopped = !Stopped;
        }
    }
}
