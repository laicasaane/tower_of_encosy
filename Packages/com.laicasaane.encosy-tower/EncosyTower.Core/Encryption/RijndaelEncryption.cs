using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using EncosyTower.Logging;

namespace EncosyTower.Encryption
{
    public sealed class RijndaelEncryption : EncryptionBase
    {
        public RijndaelEncryption([NotNull] byte[] key, [NotNull] byte[] iv, [NotNull] ILogger logger)
            : base(logger)
        {
            using var algorithm = new RijndaelManaged() { KeySize = 256 };
            Initialize(algorithm.CreateEncryptor(key, iv), algorithm.CreateDecryptor(key, iv));
        }
    }
}
