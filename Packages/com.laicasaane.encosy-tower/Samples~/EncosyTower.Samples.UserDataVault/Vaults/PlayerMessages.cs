using System.Runtime.CompilerServices;
using EncosyTower.PubSub;
using EncosyTower.Samples.UserDataVault.Shared;

namespace EncosyTower.Samples.UserDataVault.Vaults;

public readonly record struct PlayerAccessorScope
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Publish<TMsg>(TMsg msg, PublishingContext context = default)
        where TMsg : struct, IMessage
    {
        GlobalMessenger.Publisher.Scope<PlayerAccessorScope>()
            .Publish(msg, context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Publish<TMsg1, TMsg2>(TMsg1 msg1, TMsg2 msg2, PublishingContext context = default)
        where TMsg1 : struct, IMessage
        where TMsg2 : struct, IMessage
    {
        var publisher = GlobalMessenger.Publisher.Scope<PlayerAccessorScope>();
        publisher.Publish(msg1, context);
        publisher.Publish(msg2, context);
    }
}

public readonly record struct OnItemAmountUpdatedMsg(ItemId Id, Changed<int> Value) : IMessage;
