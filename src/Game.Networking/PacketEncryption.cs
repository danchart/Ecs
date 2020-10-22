using System.Collections.Generic;

namespace Game.Networking
{
    public interface IPacketEncryption
    {
        void Encrypt(PlayerId id, byte[] bytes);
        void Decrypt(PlayerId id, byte[] bytes);

        void AddPlayer(PlayerId id, byte[] key);
        void RemovePlayer(PlayerId id);
    }

    /// <summary>
    /// Completely unshippable XOR key "encryption".
    /// </summary>
    public class XorPacketEncryption : IPacketEncryption
    {
        private Dictionary<PlayerId, byte[]> PlayerIdToKey = new Dictionary<PlayerId, byte[]>(256);

        public void AddPlayer(PlayerId id, byte[] key) => this.PlayerIdToKey[id] = key;
        public void RemovePlayer(PlayerId id) => this.PlayerIdToKey.Remove(id);

        public void Decrypt(PlayerId id, byte[] bytes)
        {
            var key = this.PlayerIdToKey[id];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[i % key.Length];
            }
        }

        public void Encrypt(PlayerId id, byte[] bytes)
        {
            Decrypt(id, bytes);
        }
    }
}
