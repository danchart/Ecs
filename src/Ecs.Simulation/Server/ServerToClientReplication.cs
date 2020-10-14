using Ecs.Core;
using System;
using System.Collections.Generic;

namespace Ecs.Simulation.Server
{
    public class ServerToClientReplication
    {
        private Dictionary<Entity, int> _clients = new Dictionary<Entity, int>();

        private World World;

        private EntityQuery<ReplicateEntityComponent> _replicateEntityQuery;
        

        public ServerToClientReplication(World world)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        public void Update(float deltaTime)
        {

        }
    }
}
