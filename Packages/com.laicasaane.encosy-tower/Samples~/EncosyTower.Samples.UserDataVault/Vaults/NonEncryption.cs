using System.Diagnostics.CodeAnalysis;
using EncosyTower.Encryption;
using EncosyTower.Logging;

namespace EncosyTower.Samples.UserDataVault.Vaults;

internal sealed class NonEncryption : EncryptionBase
{
    public NonEncryption([NotNull] ILogger logger) : base(logger)
    {
    }

    public override string Encrypt(string plain)
        => plain ?? string.Empty;

    public override string Decrypt(string encrypted)
        => encrypted ?? string.Empty;
}
