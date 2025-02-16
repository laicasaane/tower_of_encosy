using EncosyTower.AtlasedSprites;
using EncosyTower.Mvvm.ComponentModel;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.AtlasedSprites
{
    public sealed partial class AtlasedSpriteProvider : MonoBehaviour, IObservableObject
    {
        [SerializeField]
        private string _atlasKey;

        [SerializeField]
        private string _spriteKey;

        [ObservableProperty]
        public AtlasedSpriteKey AtlasedSprite { get => Get_AtlasedSprite(); set => Set_AtlasedSprite(value); }

        private void Awake()
        {
            AtlasedSprite = new(_atlasKey, _spriteKey);
        }
    }
}
