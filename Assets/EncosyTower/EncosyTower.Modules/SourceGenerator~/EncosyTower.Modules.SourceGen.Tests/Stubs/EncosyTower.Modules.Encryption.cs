namespace EncosyTower.Modules.Encryption
{
    public abstract class EncryptionBase
    {
        public void Dispose() { }
    }

    public sealed class RijndaelEncryption : EncryptionBase
    {
        public RijndaelEncryption(byte[] key, byte[] iv) { }
    }
}