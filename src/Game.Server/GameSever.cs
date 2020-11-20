using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
using System;
using System.Collections.Generic;
using System.Net;

namespace Game.Server
{
    public sealed class GameServer
    {
        public readonly GameServerCommander Commander;

        private readonly ServerUdpPacketTransport _udpTransport;
        private readonly OutgoingServerChannel _outgoingChannel;
        private readonly IncomingServerChannel _incomingChannel;
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
                serverConfig.NetworkTransport.PacketEncryptor,
                serverConfig.NetworkTransport,
                serverConfig.UdpServer);
            this._outgoingChannel = new OutgoingServerChannel(
                serverConfig.NetworkTransport,
                this._udpTransport,
                serverConfig.NetworkTransport.PacketEncryptor,
                this._logger);

            this._playerConnectionManager = new PlayerConnectionManager(
                this._logger, 
                this._serverConfig.PlayerConnection);

            this._gameWorlds = new GameWorlds(
                this._logger,
                this._serverConfig,
                this._outgoingChannel,
                serverConfig.Server.WorldsCapacity);

            this._incomingChannel = new IncomingServerChannel(
                this._udpTransport,
                serverConfig.NetworkTransport.PacketEncryptor,
                new ControlPacketController(
                    this._logger,
                    this._playerConnectionManager,
                    this._outgoingChannel,
                    this._gameWorlds),
                new SimulationPacketController(
                    this._logger,
                    this._playerConnectionManager,
                    this._gameWorlds),
                this._logger);

            this._worldLoader = new GameWorldLoader();
        }

        public IPEndPoint UdpPacketEndpoint => this._serverConfig.UdpServer.HostIpEndPoint;

        public IEnumerable<GameWorld> GetWorlds()
        {
            return this._gameWorlds.GetWorlds();
        }

        public void Start()
        {
            this._incomingChannel.Start();
        }

        public bool IsRunning()
        {
            return this._gameWorlds.IsRunning();
        }

        public void StopAll()
        {
            this._incomingChannel.Stop();

            this._gameWorlds.StopAll();
        }

        public GameWorld SpawnWorld(WorldType worldType)
        {
            var factory = new GameWorldFactory(
            worldType,
            this._logger,
            this._serverConfig,
            this._outgoingChannel,
            this._worldLoader);

            return this._gameWorlds.Spawn(factory);
        }

        public bool KillWorld(WorldInstanceId id)
        {
            return this._gameWorlds.Kill(id);
        }

        public PlayerConnectionRef ConnectPlayer(
            WorldInstanceId instanceId,
            PlayerId playerId,
            byte[] encryptionKey)
        {
            // TODO: Error handling.
            var gameWorld = this._gameWorlds.Get(instanceId);

            // Create player connection.
            this._playerConnectionManager.Add(instanceId, playerId, encryptionKey);

            var playerConnectionRef = this._playerConnectionManager.GetRef(playerId);

            // Connect player to the world.
            gameWorld.Connect(playerConnectionRef);

            return playerConnectionRef;
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
