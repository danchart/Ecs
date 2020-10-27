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
            var channelManager = new ServerChannelManager(
                config.Transport,
                udpTransport,
                packetEncryption,
                logger);

            var gameWorld = new GameWorld(
                id: new WorldId(0),
                logger: logger,
                config: config,
                channelManager: channelManager);
        }
    }
}
