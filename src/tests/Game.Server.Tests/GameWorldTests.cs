using Common.Core;
using Game.Networking;
using Game.Simulation.Server;
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
            var udpTransport = new UdpPacketServerTransport(
                logger,
                config.NetworkTransport.PacketEncryptor,
                config.NetworkTransport,
                config.UdpServer);
            IPacketEncryptor packetEncryption = new XorPacketEncryptor();
            var channelManager = new ServerChannelOutgoing(
                config.NetworkTransport,
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
