using Ecs.Core;
using System.Collections.Generic;

namespace Game.Simulation.Client
{
    /// <summary>
    /// Maps server entity ID to client entity ID.
    /// </summary>
    public class NetworkEntityMap
    {
        private Dictionary<NetworkEntity, Entity> _serverToClientMap;

        public NetworkEntityMap(int capacity)
        {
            this._serverToClientMap = new Dictionary<NetworkEntity, Entity>(capacity);
        }

        public void Add(in NetworkEntity handle, in Entity clientEntity)
        {
            this._serverToClientMap[handle] = clientEntity;
        }

        public bool TryGet(in NetworkEntity handle, out Entity clientEntity)
        {
            if (this._serverToClientMap.ContainsKey(handle))
            {
                clientEntity = this._serverToClientMap[handle];

                return true;
            }

            clientEntity = default;

            return false;
        }

        public bool Remove(in NetworkEntity handle)
        {
            return this._serverToClientMap.Remove(handle);
        }
    }
}
