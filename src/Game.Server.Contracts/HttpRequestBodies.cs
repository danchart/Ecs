using System;

namespace Game.Server.Contracts
{
    [Serializable]
    public class PostPlayerConnectRequestBody
    {
        /// <summary>
        /// World type to connect player to.
        /// </summary>
        public string WorldType { get; set; }

        /// <summary>
        /// UDP packet encryption key for this session.
        /// </summary>
        public string PacketEncryptionKey { get; set; }
    }
}
