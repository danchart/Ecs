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
        private readonly ServerChannelOutgoing _channelManager;
        private readonly IGameWorldLoader _loader;

        private readonly IServerConfig _serverConfig;
        private readonly ILogger _logger;

        public GameWorldFactory(
            ILogger logger,
            IServerConfig serverConfig,
            ServerChannelOutgoing channelManager,
            IGameWorldLoader loader)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serverConfig = serverConfig ?? throw new ArgumentNullException(nameof(serverConfig));
            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
            this._loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public GameWorld CreateInstance(WorldInstanceId instanceId)
        {
            var gameWorld = new GameWorld(
                instanceId,
                this._logger,
                this._serverConfig,
                this._channelManager);

            gameWorld.Load(this._loader);

            return gameWorld;
        }
    }
}
