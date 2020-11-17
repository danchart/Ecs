using Ecs.Core;
using Game.Simulation.Core;

namespace Simulation.Core
{
    /// <summary>
    /// Synchronizes game simulation transform with the physics simulation rigid body.
    /// </summary>
    public class PhysicsSystem : SystemBase
    {
        public EntityQueryWithChangeFilter<RigidBodyComponent, TransformComponent> _transformQuery = null;

        public IPhysicsSystemProxy _physicsProxy = null;

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in this._transformQuery.GetEntities2(this.LastSystemVersion))
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                var body = this._physicsProxy.GetRigidBody(entity);

                transform.position.x = body.Position.x;
                transform.position.y = body.Position.y;
                transform.rotation = body.Angle;
            }
        }
    }
}
