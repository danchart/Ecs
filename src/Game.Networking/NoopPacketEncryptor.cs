using System;

namespace Game.Networking
{
    public class NoopPacketEncryptor : IPacketEncryptor
    {
        public PacketEncryptionResult Decrypt(byte[] key, ulong salt, byte[] encrypted, int encryptedOffset, int encryptedSize, byte[] data, int offset, out int decryptedSize)
        {
            // TODO: Implement?
            throw new NotImplementedException();
        }

        public PacketEncryptionResult Encrypt(byte[] key, ulong salt, byte[] data, int dataOffset, int dataSize, byte[] encrypted, int offset, out int encryptedSize)
        {
            // TODO: Implement?
            throw new NotImplementedException();
        }
    }
}
