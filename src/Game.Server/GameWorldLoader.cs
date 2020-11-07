using Common.Core;
using Ecs.Core;
using Game.Simulation.Core;
using System;
using System.Runtime.InteropServices;

namespace Game.Server
{
    public interface IGameWorldLoader
    {
        bool LoadWorld(World world);
    }

    public class GameWorldLoader : IGameWorldLoader
    {
        private readonly WorldType _worldType;

        public GameWorldLoader(WorldType worldType)
        {
            this._worldType = worldType;
        }

        public bool LoadWorld(World world)
        {
            // TODO: Load world based on WorldType

            // TEST: Create test objects

            int
                rows = 100,
                cols = 100;


            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var entity = world.NewEntity();

                    ref var transform = ref entity.GetComponent<TransformComponent>();

                    transform.position.x = ((float) row - (row / 2)) 

                    entity.GetComponent<MovementComponent>();

                }
            }

            return true;
        }
    }
}
