#if UNITY_EDITOR

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.AtlasedSprites
{
    //partial class AtlasedSpriteContext
    //{
    //    [MenuItem("CONTEXT/Image/Atlased Sprite Binder For This Component")]
    //    static void BindImage(MenuCommand command)
    //    {
    //        if (command.context is Image target)
    //        {
    //            Setup<ImageBinder>(target);
    //        }
    //    }

    //    [MenuItem("CONTEXT/RawImage/Atlased Sprite Binder For This Component")]
    //    static void BindRawImage(MenuCommand command)
    //    {
    //        if (command.context is RawImage target)
    //        {
    //            Setup<RawImageBinder>(target);
    //        }
    //    }

    //    public static MonoBehaviour Setup<TBinder>([NotNull] MaskableGraphic target)
    //        where TBinder : MonoBinder
    //    {
    //        var context = Undo.AddComponent<AtlasedSpriteContext>(target.gameObject);
    //        var bindingContext = Undo.AddComponent<MonoBindingContext>(target.gameObject);
    //        bindingContext.InitializeManually(context);

    //        var comp = Undo.AddComponent<TBinder>(target.gameObject);
    //        MonoBinderEditor.TryResolveNearestContext(comp);
    //        return comp;
    //    }
    //}
}

#endif
