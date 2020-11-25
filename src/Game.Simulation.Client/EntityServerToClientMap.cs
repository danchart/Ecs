using Ecs.Core;
using System.Collections.Generic;

namespace Game.Simulation.Client
{
    /// <summary>
    /// Maps server entity ID to client entity ID.
    /// </summary>
    public class EntityServerToClientMap
    {
        private Dictionary<Entity, Entity> _serverToClientMap;

        public EntityServerToClientMap(int capacity)
        {
            this._serverToClientMap = new Dictionary<Entity, Entity>(capacity);
        }

        public void Add(in Entity serverEntity, in Entity clientEntity)
        {
            this._serverToClientMap[serverEntity] = clientEntity;
        }

        public bool TryGet(in Entity serverEntity, out Entity clientEntity)
        {
            if (this._serverToClientMap.ContainsKey(serverEntity))
            {
                clientEntity = this._serverToClientMap[serverEntity];

                return true;
            }

            clientEntity = default;

            return false;
        }

        public bool Remove(in Entity serverEntity)
        {
            return this._serverToClientMap.Remove(serverEntity);
        }
    }
}
