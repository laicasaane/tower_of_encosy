#if UNITY_ADDRESSABLES

/// <auto-generated>
///*****************************************************************///
///                                                                 ///
/// This file is auto-generated by AnyAddressablesAdaptersGenerator ///
///                   DO NOT manually modify it!                    ///
///                                                                 ///
///*****************************************************************///
/// </auto-generated>

#pragma warning disable

using System;
using EncosyTower.Modules.AddressableKeys;
using EncosyTower.Modules.Mvvm.ViewBinding;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.AddressableKeys
{
    #region    SPRITE
    #endregion ======

    [Serializable]
    [Label("AddressableKey<Sprite>.Load()", "Default")]
    [Adapter(sourceType: typeof(AddressableKey<Sprite>), destType: typeof(Sprite), order: 0)]
    public sealed class AddressableKeyTypedToSpriteAdapter : AddressableKeyTypedAdapter<Sprite> { }

    [Serializable]
    [Label("AddressableKey.Load<Sprite>()", "Default")]
    [Adapter(sourceType: typeof(AddressableKey), destType: typeof(Sprite), order: 1)]
    public sealed class AddressableKeyUntypedToSpriteAdapter : AddressableKeyUntypedAdapter<Sprite> { }

    [Serializable]
    [Label("Addressables.Load<Sprite>(string)", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(Sprite), order: 2)]
    public sealed class AddressableStringToSpriteAdapter : AddressableStringAdapter<Sprite> { }

    #region    GAMEOBJECT
    #endregion ==========

    [Serializable]
    [Label("AddressableKey<GameObject>.Load()", "Default")]
    [Adapter(sourceType: typeof(AddressableKey<GameObject>), destType: typeof(GameObject), order: 0)]
    public sealed class AddressableKeyTypedToGameObjectAdapter : AddressableKeyTypedAdapter<GameObject> { }

    [Serializable]
    [Label("AddressableKey.Load<GameObject>()", "Default")]
    [Adapter(sourceType: typeof(AddressableKey), destType: typeof(GameObject), order: 1)]
    public sealed class AddressableKeyUntypedToGameObjectAdapter : AddressableKeyUntypedAdapter<GameObject> { }

    [Serializable]
    [Label("Addressables.Load<GameObject>(string)", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(GameObject), order: 2)]
    public sealed class AddressableStringToGameObjectAdapter : AddressableStringAdapter<GameObject> { }

    #region    AUDIOCLIP
    #endregion =========

    [Serializable]
    [Label("AddressableKey<AudioClip>.Load()", "Default")]
    [Adapter(sourceType: typeof(AddressableKey<AudioClip>), destType: typeof(AudioClip), order: 0)]
    public sealed class AddressableKeyTypedToAudioClipAdapter : AddressableKeyTypedAdapter<AudioClip> { }

    [Serializable]
    [Label("AddressableKey.Load<AudioClip>()", "Default")]
    [Adapter(sourceType: typeof(AddressableKey), destType: typeof(AudioClip), order: 1)]
    public sealed class AddressableKeyUntypedToAudioClipAdapter : AddressableKeyUntypedAdapter<AudioClip> { }

    [Serializable]
    [Label("Addressables.Load<AudioClip>(string)", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(AudioClip), order: 2)]
    public sealed class AddressableStringToAudioClipAdapter : AddressableStringAdapter<AudioClip> { }

}

#endif
