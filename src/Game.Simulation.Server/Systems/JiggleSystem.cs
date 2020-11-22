using Ecs.Core;
using Game.Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    /// <summary>
    /// Jiggles most repliated entities every frame, for replication testing.
    /// </summary>
    public class JiggleSystem : SystemBase
    {
        public EntityQuery<ReplicatedComponent, TransformComponent>.Exclude<PlayerComponent> Query = null;

        private Random _random = new Random();

        public override void OnUpdate(float deltaTime)
        {
            foreach (int index in Query)
            {
                ref var transform = ref Query.Get2(index);

                transform.position.x += ((float)50 - this._random.Next(0, 100)) / 1000.0f;
                transform.position.y += ((float)50 - this._random.Next(0, 100)) / 1000.0f;
            }
        }
    }
}
 