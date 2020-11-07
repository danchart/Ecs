using Ecs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server
{
    public interface IGameWorldFactory
    {
        GameWorld CreateInstance();
    }

    public sealed class GameWorldFactory : IGameWorldFactory
    {
        public GameWorldFactory()
        {

        }

        public GameWorld CreateInstance()
        {
            new GameWorld(
                new WorldId(worldId),
                this._logger,
                this._serverConfig,
                this._channelManager);
        }
    }
}
