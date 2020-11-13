using Common.Core;
using Common.Core.Numerics;
using Ecs.Core;
using Game.Simulation.Core;
using Simulation.Core;

namespace Game.Server
{
    public interface IGameWorldLoader
    {
        bool LoadWorld(WorldType worldType, World world, IPhysicsWorld physicsWorld);
    }

    public class GameWorldLoader : IGameWorldLoader
    {
        public GameWorldLoader()
        {
        }

        public bool LoadWorld(WorldType worldType, World world, IPhysicsWorld physicsWorld)
        {
            // TODO: Load world based on WorldType


            const int
                rows = 100,
                cols = 100;
            const float unitSize = 0.1f;
            const float halfUnitSize = 0.5f * unitSize;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var entity = world.NewEntity();

                    ref var transform = ref entity.GetComponent<TransformComponent>();

                    float x = unitSize * ((float)row - ((float)rows / 2));
                    float y = unitSize * ((float)col - ((float)cols / 2));

                    transform.position.x = x;
                    transform.position.y = y;
                    transform.rotation = 0;

                    ref var movement = ref entity.GetComponent<MovementComponent>();

                    movement.velocity = Vector2.Zero;

                    //physicsWorld.AddCircle(entity, isStatic: false, transform.position, 0, 0.5f * unitSize);

                    // Create square rigid body.
                    physicsWorld.AddPolygon(
                        entity, 
                        isStatic: false, 
                        originWS: transform.position, 
                        rotation: 0, 
                        vertices: new Vector2[]
                        {
                            new Vector2(x - halfUnitSize, y + halfUnitSize),
                            new Vector2(x + halfUnitSize, y + halfUnitSize),
                            new Vector2(x + halfUnitSize, y - halfUnitSize),
                            new Vector2(x - halfUnitSize, y - halfUnitSize),
                        });
                }
            }

            return true;
        }
    }
}
