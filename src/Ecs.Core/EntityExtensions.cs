﻿using System;

namespace Ecs.Core
{
    public static class EntityExtensions
    {
        public static ref T GetComponent<T>(in this Entity entity) where T : struct
        {
            ref var entityData = ref entity.World.GetAndValidateEntityData(entity);

            var componentPoolIndex = ComponentType<T>.ComponentPoolIndex;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].PoolIndex == componentPoolIndex)
                {
                    // Found component
                    return ref ((ComponentPool<T>)entity.World.ComponentPools[componentPoolIndex]).GetItem(entityData.Components[i].ItemIndex);
                }
            }

            // Create component

            if (entityData.Components.Length == entityData.ComponentCount)
            {
                Array.Resize(ref entityData.Components, entityData.ComponentCount * 2);
            }

            var pool = entity.World.GetPool<T>();
            var index = pool.New();
            entityData.Components[entityData.ComponentCount] = new World.EntityData.ComponentData
            {
                PoolIndex = componentPoolIndex,
                ItemIndex = index
            };
            entityData.ComponentCount++;

            entity.World.UpdateEntityQueries(componentPoolIndex, entity, entityData, isDelete: false);

            return ref pool.GetItem(index);
        }

        public static bool HasComponent<T>(in this Entity entity) where T : struct
        {
            ref var entityData = ref entity.World.GetAndValidateEntityData(entity);

            var componentPoolIndex = ComponentType<T>.ComponentPoolIndex;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].PoolIndex == componentPoolIndex)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool RemoveComponent<T>(in this Entity entity) where T : struct
        {
            bool wasRemoved = false;

            var componentPoolIndex = ComponentType<T>.ComponentPoolIndex;

            ref var entityData = ref entity.World.GetAndValidateEntityData(entity);

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].PoolIndex == componentPoolIndex)
                {
                    entity.World.UpdateEntityQueries(componentPoolIndex, entity, entityData, isDelete: true);

                    entity.World.ComponentPools[componentPoolIndex].Free(entityData.Components[i].ItemIndex);

                    if (i < entityData.ComponentCount - 1)
                    {
                        // Move the last item to the removed position
                        entityData.Components[i] = entityData.Components[entityData.ComponentCount - 1];
                    }

                    entityData.ComponentCount--;

                    wasRemoved = true;

                    break;
                }
            }

            // Free entities with no more components.
            if (entityData.ComponentCount == 0)
            {
                entity.World.FreeEntityData(entity.Id, ref entityData);
            }

            return wasRemoved;
        }

        public static ComponentRef<T> Reference<T>(in this Entity entity) where T : struct
        {
            ref var entityData = ref entity.World.GetEntityData(entity);

            var poolIndex = ComponentType<T>.ComponentPoolIndex;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].PoolIndex == poolIndex)
                {
                    return 
                        ((ComponentPool<T>)entity.World.ComponentPools[poolIndex])
                        .Reference(entityData.Components[i].ItemIndex);
                }
            }

            return default;
        }

        public static void Free(in this Entity entity)
        {
            ref var entityData = ref entity.World.GetAndValidateEntityData(entity);

            for (int i = entityData.ComponentCount - 1; i >= 0; i--)
            {
                var componentPoolIndex = entityData.Components[i].PoolIndex;

                entity.World.UpdateEntityQueries(componentPoolIndex, entity, entityData, isDelete: true);
                entity.World.ComponentPools[componentPoolIndex].Free(entityData.Components[i].ItemIndex);
                entityData.ComponentCount--;
            }

            entity.World.FreeEntityData(entity.Id, ref entityData);
        }


        /// <summary>
        /// Returns true if the entity has been freed and is no longer usable.
        /// </summary>
        public static bool IsFreed(in this Entity entity)
        {
            ref var entityData = ref entity.World.GetEntityData(entity);

            return
                entityData.Version != entity.Version ||
                entityData.ComponentCount == 0;
        }
    }
}
