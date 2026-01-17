using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using EncosyTower.Logging;

namespace EncosyTower.Encryption
{
    public sealed class AesEncryption : EncryptionBase
    {
        public AesEncryption([NotNull] string password, [NotNull] string saltKey, [NotNull] ILogger logger)
            : base(logger)
        {
            // 10000 iterations is recommended by many security experts
            // But this library focuses on providing cross-platform solution,
            // including low processing-power devices such as mobile
            // So I stay at 100 iterations
            // You can freely increase this value to improve security
            int numberIteration = 100;

            using var rfc2898 = new Rfc2898DeriveBytes(
                  password
                , Encoding.UTF8.GetBytes(saltKey)
                , numberIteration
            );

            using var algorithm = new AesManaged {
                Key = rfc2898.GetBytes(16),
                IV = rfc2898.GetBytes(16),
            };

            Initialize(algorithm.CreateEncryptor(), algorithm.CreateDecryptor());
        }
    }
}
