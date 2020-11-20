using Common.Core;
using Game.Networking;
using System;

namespace Game.Server
{
    public interface IGameWorldFactory
    {
        GameWorld CreateInstance(WorldInstanceId instanceId);
    }

    public sealed class GameWorldFactory : IGameWorldFactory
    {
        private readonly WorldType _worldType;
        private readonly OutgoingServerChannel _channelManager;
        private readonly IGameWorldLoader _worldLoader;

        private readonly IServerConfig _serverConfig;
        private readonly ILogger _logger;

        public GameWorldFactory(
            WorldType worldType,
            ILogger logger,
            IServerConfig serverConfig,
            OutgoingServerChannel channelManager,
            IGameWorldLoader loader)
        {
            this._worldType = worldType;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serverConfig = serverConfig ?? throw new ArgumentNullException(nameof(serverConfig));
            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
            this._worldLoader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public GameWorld CreateInstance(WorldInstanceId instanceId)
        {
            var gameWorld = new GameWorld(
                this._worldType,
                instanceId,
                this._logger,
                this._serverConfig,
                this._channelManager,
                this._worldLoader);

            return gameWorld;
        }
    }
}
