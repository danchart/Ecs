using Ecs.Core;
using Game.Simulation.Core;

namespace Simulation.Core.Systems
{
    /// <summary>
    /// Synchronizes game simulation transform with the physics simulation rigid body.
    /// </summary>
    public class PhysicsSystem : SystemBase
    {
        public EntityQuery<RigidBodyComponent, TransformComponent> _transformQuery = null;

        public IPhysicsSystemProxy PhysicsProxy = null;

        public override void OnUpdate(float deltaTime)
        {

        }
    }
}
