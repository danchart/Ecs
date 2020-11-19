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

            foreach (var index in this._transformQuery)
            {
                var entity = this._transformQuery.GetEntity(index);
                var body = this._physicsProxy.GetRigidBody(entity);

                ref readonly TransformComponent transformReadOnly = ref this._transformQuery.Get2Readonly(index);

                if (transformReadOnly.IsNotEqualTo(
                    new Common.Core.Numerics.Vector2(
                        body.Position.x, 
                        body.Position.y), 
                    body.Angle))
                {
                    ref var transform = ref entity.GetComponent<TransformComponent>();

                    transform.position.x = body.Position.x;
                    transform.position.y = body.Position.y;
                    transform.rotation = body.Angle;
                }
            }
        }
    }
}
