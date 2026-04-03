namespace EncosyTower.Samples.UserDataVault.Shared;

public readonly record struct Changed<T>(T New, T Previous);
