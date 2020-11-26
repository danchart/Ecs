using Ecs.Core;
using Game.Simulation.Core;
using Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Jiggles most repliated entities every frame, for replication testing.
    /// </summary>
    public class JiggleSystem : SystemBase
    {
        public EntityQuery<ReplicatedComponent, TransformComponent>.Exclude<PlayerComponent> Query = null;

        private IPhysicsSystemProxy _physicsProxy = null;

        private Random _random = new Random();

        public override void OnUpdate(float deltaTime)
        {
            foreach (int index in Query)
            {
                var entity = this.Query.GetEntity(index);
                var body = this._physicsProxy.GetRigidBody(entity);

                body.AddForce(
                    new Volatile.Vector2(
                        ((float)50 - this._random.Next(0, 100)) / 10000.0f,
                        ((float)50 - this._random.Next(0, 100)) / 10000.0f
                        ));
            }
        }
    }
}
 