using Common.Core;
using Game.Networking;
using System;
using System.Net;

namespace Game.Server
{
    public sealed class GameServer
    {
        public readonly GameServerCommander Commander;

        private readonly ServerUdpPacketTransport _udpTransport;
        private readonly ServerChannelOutgoing _channelOutgoing;
        private readonly ServerChannelIncoming _channelIncoming;
        private readonly PlayerConnectionManager _playerConnectionManager;

        private readonly IGameWorldLoader _worldLoader;

        private readonly GameWorlds _gameWorlds;

        private readonly ILogger _logger;
        private readonly IServerConfig _serverConfig;

        public GameServer(
            IServerConfig serverConfig,
            ILogger logger)
        {
            this._serverConfig = serverConfig  ?? throw new ArgumentNullException(nameof(serverConfig));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.Commander = new GameServerCommander(this);

            this._udpTransport = new ServerUdpPacketTransport(
                this._logger,
                serverConfig.Transport.UdpPacket);
            this._channelOutgoing = new ServerChannelOutgoing(
                serverConfig.Transport,
                this._udpTransport,
                serverConfig.Transport.PacketEncryption,
                this._logger);

            this._playerConnectionManager = new PlayerConnectionManager(
                this._logger, 
                this._serverConfig.PlayerConnection);

            this._gameWorlds = new GameWorlds(
                this._logger,
                this._serverConfig,
                this._channelOutgoing,
                serverConfig.Server.WorldsCapacity);

            this._channelIncoming = new ServerChannelIncoming(
                this._udpTransport,
                serverConfig.Transport.PacketEncryption,
                new ControlPacketController(
                    this._logger,
                    this._playerConnectionManager,
                    this._channelOutgoing,
                    this._gameWorlds),
                new SimulationPacketController(
                    this._logger,
                    this._playerConnectionManager,
                    this._gameWorlds),
                this._logger);

            this._worldLoader = new GameWorldLoader();
        }

        public void Start()
        {
            this._channelIncoming.Start();
        }

        public bool IsRunning()
        {
            return this._gameWorlds.IsRunning();
        }

        public void StopAll()
        {
            this._channelIncoming.Stop();

            this._gameWorlds.StopAll();
        }

        public WorldInstanceId SpawnWorld(WorldType worldType)
        {
            var factory = new GameWorldFactory(
            worldType,
            this._logger,
            this._serverConfig,
            this._channelOutgoing,
            this._worldLoader);

            return this._gameWorlds.Spawn(factory);
        }

        public bool KillWorld(WorldInstanceId id)
        {
            return this._gameWorlds.Kill(id);
        }

        public bool ConnectPlayer(
            WorldInstanceId instanceId,
            PlayerId playerId,
            byte[] encryptionKey,
            IPEndPoint ipEndPoint)
        {
            // TODO: Error handling.
            var gameWorld = this._gameWorlds.Get(instanceId);

            // Create player connection.
            this._playerConnectionManager.Add(instanceId, playerId, encryptionKey, ipEndPoint);

            var playerConnectionRef = this._playerConnectionManager.GetRef(playerId);

            // Connect player to the world.
            gameWorld.Connect(playerConnectionRef);

            return true;
        }

        public bool DisconnectPlayer(
            WorldInstanceId instanceId,
            PlayerId playerId)
        {
            // TODO: Error handling.

            var gameWorld = this._gameWorlds.Get(instanceId);

            var playerConnectionRef = this._playerConnectionManager.GetRef(playerId);

            gameWorld.Disconnect(playerConnectionRef);

            // Remove player connection,  
            this._playerConnectionManager.Remove(playerId);

            return true;
        }
    }
}
