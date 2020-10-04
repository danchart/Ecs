using System;

namespace Ecs.Core
{
    public class World
    {
        public IComponentPool[] ComponentPools = new IComponentPool[EcsConstants.InitialComponentPoolCount];

        private EntityData[] _entities = new EntityData[EcsConstants.InitialEntityCount];

        private int[] _freeEntityIds = new int[EcsConstants.InitialEntityCount];

        private int _entityCount = 0;
        private int _freeEntityCount = 0;

        public Entity NewEntity()
        {
            var entity = new Entity
            {
                World = this,
                Id = GetNextEntityId(),
            };

            ref var entityData = ref _entities[entity.Id];

            entityData.ComponentCount = 0;
            entityData.ComponentIndices = new int[EcsConstants.InitialEntityComponentCount];
            entityData.ComponentPoolIndices = new int[EcsConstants.InitialEntityComponentCount];

            return entity;
        }

        public ref EntityData GetEntityData(Entity entity)
        {
            return ref _entities[entity.Id];
        }

        public ref T New<T>() where T : struct
        {
            var pool = GetPool<T>();
            var index = pool.New();

            return ref pool.GetItem(index);
        }

        public void Destroy<T>(ComponentRef<T> dataRef) where T : struct
        {
            var pool = GetPool<T>();
            pool.Free(dataRef.ItemIndex);
        }

        public ComponentPool<T> GetPool<T>() where T : struct
        {
            var poolIndex = ComponentType<T>.ComponentPoolIndex;

            if (ComponentPools.Length < poolIndex)
            {
                var len = ComponentPools.Length * 2;

                while (len <= poolIndex)
                {
                    len <<= 1;
                }
                Array.Resize(ref ComponentPools, len);
            }

            var pool = (ComponentPool<T>)ComponentPools[poolIndex];

            if (pool == null)
            {
                pool = new ComponentPool<T>();
                ComponentPools[poolIndex] = pool;
            }

            return pool;
        }

        private int GetNextEntityId()
        {
            if (_freeEntityCount > 0)
            {
                return _freeEntityIds[_freeEntityCount--];
            }

            if (_entityCount == _entities.Length)
            {
                Array.Resize(ref _entities, _entityCount * 2);
            }

            return _entityCount++;
        }


        public struct EntityData
        {
            public int[] ComponentPoolIndices;
            public int[] ComponentIndices;
            public int ComponentCount;
        }
    }
}