using Ecs.Core;
using Game.Simulation.Core;

namespace Simulation.Core
{
    /// <summary>
    /// Synchronizes game simulation transform with the physics simulation rigid body.
    /// </summary>
    public class PhysicsSystem : SystemBase
    {
        private EntityQuery<RigidBodyComponent, TransformComponent>.Exclude<IsDisabledComponent> _transformQuery = null;

        private IPhysicsSystemProxy _physicsProxy = null;

        public override void OnUpdate(float deltaTime)
        {
            this._physicsProxy.Run(deltaTime);

            foreach (var entity in this._transformQuery)
            {
                ref readonly var transform = ref entity.GetReadOnlyComponent<TransformComponent>();
                var body = this._physicsProxy.GetRigidBody(entity);

                if (transform.IsNotEqualTo(ToVolt body.Position))

                transform.position.x = body.Position.x;
                transform.position.y = body.Position.y;
                transform.rotation = body.Angle;


                transform.position.x = body.Position.x;
                transform.position.y = body.Position.y;
                transform.rotation = body.Angle;
            }
        }
    }
}
