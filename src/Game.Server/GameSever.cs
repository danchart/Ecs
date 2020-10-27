using Common.Core;
using Game.Networking;
using System;

namespace Game.Server
{
    public sealed class GameServer
    {
        private readonly ServerUdpPacketTransport _udpTransport;
        private readonly ServerChannelManager _channelManager;
        private readonly PlayerConnectionManager _playerConnectionManager;
        private readonly ClientControlPlaneController _clientControlPlaneController;

        private readonly GameWorlds _gameWorlds;

        private readonly ILogger _logger;
        private readonly IServerConfig _serverConfig;

        public GameServer(
            IServerConfig serverConfig,
            ILogger logger)
        {
            this._serverConfig = serverConfig  ?? throw new ArgumentNullException(nameof(serverConfig));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this._playerConnectionManager = new PlayerConnectionManager(this._logger, this._serverConfig.PlayerConnection);
            this._clientControlPlaneController = new ClientControlPlaneController(this._logger, this._playerConnectionManager, )

            this._udpTransport = new ServerUdpPacketTransport(
                this._logger, 
                serverConfig.Transport.UdpPacket);
            this._channelManager = new ServerChannelManager(
                serverConfig.Transport, 
                this._udpTransport, 
                serverConfig.Transport.PacketEncryption);
            this._gameWorlds = new GameWorlds(
                this._logger, 
                this._serverConfig, 
                this._channelManager, 
                serverConfig.World.WorldsCapacity);
        }

        public bool IsRunning()
        {
            return this._gameWorlds.IsRunning();
        }

        public void StopAll()
        {
            this._gameWorlds.StopAll();
        }

        public WorldId SpawnWorld()
        {
            return this._gameWorlds.Spawn();
        }
    }
}
