using System;

namespace Ecs.Core
{
    public class World
    {
        public readonly EcsConfig Config;

        public IComponentPool[] ComponentPools;

        private EntityData[] _entities;
        private int[] _freeEntityIds;

        private int _entityCount = 0;
        private int _freeEntityCount = 0;

        public World(EcsConfig config)
        {
            Config = config;

            ComponentPools = new IComponentPool[Config.InitialComponentPoolSize];
            _entities = new EntityData[Config.InitialEntityPoolSize];
            _freeEntityIds = new int[Config.InitialEntityPoolSize];
        }

        public Entity NewEntity()
        {
            var entity = new Entity
            {
                World = this,
                Id = GetNextEntityId(),
            };

            ref var entityData = ref _entities[entity.Id];

            entityData.ComponentCount = 0;
            entityData.Components = new EntityData.ComponentData[Config.InitialEntityComponentCount];

            return entity;
        }

        public ref EntityData GetEntityData(Entity entity)
        {
            return ref _entities[entity.Id];
        }

        //public ref T NewComponent<T>() where T : struct
        //{
        //    var pool = GetPool<T>();
        //    var index = pool.New();

        //    return ref pool.GetItem(index);
        //}

        //public void DestroyComponent<T>(ComponentRef<T> dataRef) where T : struct
        //{
        //    var pool = GetPool<T>();
        //    pool.Free(dataRef.ItemIndex);
        //}

        public ComponentPool<T> GetPool<T>() where T : struct
        {
            var poolIndex = ComponentType<T>.ComponentPoolIndex;

            if (ComponentPools.Length < poolIndex)
            {
                var len = ComponentPools.Length * 2;

                while (len <= poolIndex)
                {
                    len *= 2;
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
            public ComponentData[] Components;
            public int ComponentCount;

            public struct ComponentData
            {
                public int PoolIndex;
                public int ItemIndex;
            }
        }
    }
}