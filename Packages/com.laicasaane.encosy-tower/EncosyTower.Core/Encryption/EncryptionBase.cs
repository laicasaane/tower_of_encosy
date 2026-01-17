using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EncosyTower.IO;
using EncosyTower.Logging;

namespace EncosyTower.Encryption
{
    public abstract class EncryptionBase : IDisposable
    {
        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;

        protected EncryptionBase([NotNull] ILogger logger)
        {
            Logger = logger;
        }

        public bool IsInitialized => _encryptor != null && _decryptor != null;

        protected ILogger Logger { get; }

        protected void Initialize([NotNull] ICryptoTransform encryptor, [NotNull] ICryptoTransform decryptor)
        {
            _encryptor = encryptor;
            _decryptor = decryptor;
        }

        public string Encrypt(string plain)
        {
            if (plain == null)
            {
                return string.Empty;
            }

            ThrowIfNotInitialized(IsInitialized);

            try
            {
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, _encryptor, CryptoStreamMode.Write);

                // Encrypt data
                var data = Encoding.UTF8.GetBytes(plain);
                cryptoStream.Write(data.AsSpan());
                cryptoStream.FlushFinalBlock();

                // Convert encrypted data to base64 string
                return Convert.ToBase64String(memoryStream.AsSpan());
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return string.Empty;
            }
        }

        public string Decrypt(string encrypted)
        {
            if (encrypted == null)
            {
                return string.Empty;
            }

            ThrowIfNotInitialized(IsInitialized);

            try
            {
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, _decryptor, CryptoStreamMode.Write);

                // Convert from base64 string to bytes and decrypt
                var data = Convert.FromBase64String(encrypted);
                cryptoStream.Write(data.AsSpan());
                cryptoStream.FlushFinalBlock();

                // Convert decrypted message to string
                return Encoding.UTF8.GetString(memoryStream.AsSpan());
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return string.Empty;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            _encryptor?.Dispose();
            _encryptor = null;

            _decryptor?.Dispose();
            _decryptor = null;

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private static void ThrowIfNotInitialized([DoesNotReturnIf(false)] bool isInitialized)
        {
            if (isInitialized == false)
            {
                throw new InvalidOperationException("Encryption is not initialized.");
            }
        }
    }
}
