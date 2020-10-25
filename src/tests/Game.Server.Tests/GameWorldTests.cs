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

            var playerConnections = new PlayerConnectionManager(config.PlayerConnection);

            var gameWorld = new GameWorld(
                id: new WorldId(0),
                logger: logger,
                config: config,
                playerConnections: playerConnections);
        }
    }
}
