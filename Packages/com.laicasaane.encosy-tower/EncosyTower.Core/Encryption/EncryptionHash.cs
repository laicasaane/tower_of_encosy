using System.Text;
using System.Security.Cryptography;

namespace EncosyTower.Encryption
{
    public enum EncryptionHash
    {
        MD5,
        SHA256,
        SHA512
    }

    public static class EncryptionHashTypeExtensions
    {
        private const string FORMAT = "x2";

        public static string ComputeHash(this EncryptionHash self, string input)
        {
            input ??= string.Empty;

            try
            {
                using var algorithm = GetAlgorithm(self);
                byte[] crypto = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hash = new StringBuilder();

                for (int i = 0; i < crypto.Length; i++)
                {
                    hash.Append(crypto[i].ToString(FORMAT));
                }

                return hash.ToString();
            }
            catch
            {
                return input;
            }
        }

        private static HashAlgorithm GetAlgorithm(this EncryptionHash self)
        {
            return self switch {
                EncryptionHash.MD5 => MD5.Create(),
                EncryptionHash.SHA256 => SHA256.Create(),
                EncryptionHash.SHA512 => SHA512.Create(),
                _ => null,
            };
        }
    }
}
