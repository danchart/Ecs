using Common.Core;
using Common.Core.Numerics;
using Ecs.Core;
using Game.Simulation.Core;

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

            // TEST: Create test grid

            const int
                rows = 100,
                cols = 100;
            const float unitSize = 0.1f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var entity = world.NewEntity();

                    ref var transform = ref entity.GetComponent<TransformComponent>();

                    transform.position.x = ((float)row - (row / 2)) * unitSize;
                    transform.position.y = ((float)col - (col / 2)) * unitSize;
                    transform.rotation = 0;

                    ref var movement = ref entity.GetComponent<MovementComponent>();

                    movement.velocity = Vector2.Zero;
                }
            }

            return true;
        }
    }
}
