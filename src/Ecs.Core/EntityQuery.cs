using System;

namespace Ecs.Core
{
    public class EntityQuery
    {
        private int[] _entityIndices = new int[32];
        private int _entityCount = 0;

        private int[] _componentTypes;

        public void AddEntity(in Entity entity)
        {
            if (_entityIndices.Length == _entityCount)
            {
                Array.Resize(ref _entityIndices, 2 * _entityCount);)
            }

            _entityIndices[_entityCount++] = entity.Id;
        }
    }
}
