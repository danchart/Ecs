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

        public IPhysicsSystemProxy _physicsProxy = null;

        public override void OnUpdate(float deltaTime)
        {
            //foreach (ref var transform in this._transformQuery.GetComponents2())
            foreach (var entity in this._transformQuery)
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
