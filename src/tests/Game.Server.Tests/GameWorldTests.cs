﻿using Common.Core;
using Game.Networking;
using Test.Common;
using Xunit;

namespace Game.Server.Tests
{
    public class GameWorldTests
    {
        [Fact]
        public void Test()
        {
            var logger = new TestLogger();
            var config = new DefaultServerConfig();

            var playerConnections = new PlayerConnectionManager(logger, config.PlayerConnection);
            var udpTransport = new ServerUdpPacketTransport(logger, config.Transport.UdpPacket);
            IPacketEncryption packetEncryption = new XorPacketEncryption();
            var channelManager = new ServerChannelOutgoing(
                config.Transport,
                udpTransport,
                packetEncryption,
                logger);
            IGameWorldLoader gameWorldLoader = new GameWorldLoader();

            var gameWorld = new GameWorld(
                worldType: WorldType.New(),
                id: WorldInstanceId.New(),
                logger: logger,
                config: config,
                channelManager: channelManager,
                gameWorldLoader: gameWorldLoader);
        }
    }
}
