using System;
using System.Diagnostics;

namespace Game.Networking
{
    public interface IPacketEncryption
    {
        PacketEncryptionResult Encrypt(
            byte[] key, 
            ulong salt, 
            byte[] data,
            int dataOffset,
            int dataSize,
            byte[] encrypted, 
            int offset, 
            out int encryptedSize);
        PacketEncryptionResult Decrypt(
            byte[] key, 
            ulong salt, 
            byte[] encrypted, 
            int encryptedOffset,
            int encryptedSize,
            byte[] data, 
            int offset, 
            out int decryptedSize);

        //void AddPlayer(PlayerId id, byte[] key);
        //void RemovePlayer(PlayerId id);
    }

    public enum PacketEncryptionResult
    {
        Success = 0,
        DecryptSaltMismatch,
        OutofBufferSpace,
    }

    /// <summary>
    /// Completely unshippable XOR key "encryption".
    /// </summary>
    public class XorPacketEncryption : IPacketEncryption
    {
        //private Dictionary<PlayerId, byte[]> PlayerIdToKey = new Dictionary<PlayerId, byte[]>(256);

        //public void AddPlayer(PlayerId id, byte[] key) => this.PlayerIdToKey[id] = key;
        //public void RemovePlayer(PlayerId id) => this.PlayerIdToKey.Remove(id);

        public PacketEncryptionResult Decrypt(
            byte[] key, 
            ulong salt, 
            byte[] encrypted,
            int encryptedOffset,
            int encryptedSize,
            byte[] data, 
            int offset, 
            out int decryptedSize)
        {
            //var key = this.PlayerIdToKey[id];

            Debug.Assert(encryptedOffset + encryptedSize <= encrypted.Length);
            Debug.Assert(offset < data.Length);

            decryptedSize = 0;

            var dataSize = data.Length;

            // Decrypt salt value

            var saltByteCount = sizeof(ulong);
            var saltDecrypted = new byte[saltByteCount];

            for (int i = 0; i < saltByteCount; i++)
            {
                saltDecrypted[i] = (byte)(encrypted[encryptedOffset + i] ^ key[i % key.Length]);
            }

            var saltFromPacket = BitConverter.ToUInt32(saltDecrypted, 0);

            if (saltFromPacket != salt)
            {
                return PacketEncryptionResult.DecryptSaltMismatch;
            }

            // Decrypt data

            var encryptedDataSize = encryptedSize - saltByteCount;

            for (int i = 0; i < encryptedDataSize; i++)
            {
                if (i + offset == dataSize)
                {
                    return PacketEncryptionResult.OutofBufferSpace;
                }

                data[i + offset] = (byte)(encrypted[i + encryptedOffset + saltByteCount] ^ key[i % key.Length]);

                decryptedSize++;
            }

            return PacketEncryptionResult.Success;
        }

        public PacketEncryptionResult Encrypt(
            byte[] key,
            ulong salt,
            byte[] data,
            int dataOffset,
            int dataSize,
            byte[] encrypted,
            int encryptedOffset,
            out int encryptedSize)
        {
            Debug.Assert(dataOffset + dataSize <= data.Length);
            Debug.Assert(encryptedOffset < encrypted.Length);

            // Encrypt salt value

            encryptedSize = 0;

            var encryptedBufferSize = encrypted.Length;

            var saltBytes = BitConverter.GetBytes(salt);
            var saltBytesLength = saltBytes.Length;

            if (saltBytesLength >= encryptedBufferSize)
            {
                return PacketEncryptionResult.OutofBufferSpace;
            }

            for (int i = 0; encryptedOffset + i < encryptedBufferSize && i < saltBytesLength; i++)
            {
                encrypted[encryptedOffset + encryptedSize++] = (byte)(saltBytes[i] ^ key[i % key.Length]);
            }

            // Encrypt data

            for (int i = 0; i < dataSize; i++)
            {
                if (saltBytesLength + i == encryptedBufferSize)
                {
                    return PacketEncryptionResult.OutofBufferSpace;
                }

                encrypted[encryptedOffset + encryptedSize++] = (byte)(data[dataOffset + i] ^ key[i % key.Length]);
            }

            return PacketEncryptionResult.Success;
        }
    }
}
