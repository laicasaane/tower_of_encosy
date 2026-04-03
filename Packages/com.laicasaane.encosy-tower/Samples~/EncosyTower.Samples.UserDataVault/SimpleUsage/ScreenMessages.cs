using EncosyTower.PubSub;

namespace EncosyTower.Samples.UserDataVault.SimpleUsage;

internal readonly record struct ScreenScope();

internal readonly record struct ShowMainMenuScreenMsg() : IMessage;

internal readonly record struct ShowLobbyScreenMsg() : IMessage;
