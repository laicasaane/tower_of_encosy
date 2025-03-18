using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using EncosyTower.Logging;
using EncosyTower.IO;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.Encryption
{
    public abstract class EncryptionBase : IDisposable
    {
        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;
        private bool _disposedValue;

        protected EncryptionBase([NotNull] ILogger logger)
        {
            Logger = logger;
        }

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
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _encryptor.Dispose();
                _decryptor.Dispose();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
