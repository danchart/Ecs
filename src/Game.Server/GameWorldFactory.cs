using Common.Core;
using Game.Networking;
using System;

namespace Game.Server
{
    public interface IGameWorldFactory
    {
        GameWorld CreateInstance(WorldInstanceId instanceId);
    }

    // TODO: Need to spanw specific world type!
    public sealed class GameWorldFactory : IGameWorldFactory
    {
        private readonly ServerChannelOutgoing _channelManager;

        private readonly IServerConfig _serverConfig;
        private readonly ILogger _logger;

        public GameWorldFactory(
            WorldType worldType,
            ILogger logger,
            IServerConfig serverConfig,
            ServerChannelOutgoing channelManager)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serverConfig = serverConfig ?? throw new ArgumentNullException(nameof(serverConfig));
            this._channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
        }

        public GameWorld CreateInstance(WorldInstanceId instanceId)
        {
            return new GameWorld(
                instanceId,
                this._logger,
                this._serverConfig,
                this._channelManager);
        }
    }
}
