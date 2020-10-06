﻿using System;

namespace Ecs.Core
{
    public static class EntityExtensions
    {
        public static Version GetComponentVersion<T>(in this Entity entity) where T : struct
        {
            ref readonly var entityData = ref entity.World.GetCheckedEntityData(entity);

            var componentTypeIndex = ComponentType<T>.Index;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == componentTypeIndex)
                {
                    // Found component
                    return entityData.Components[i].Version;
                }
            }

            return default;
        }

        public static void Dirty<T>(in this Entity entity) where T : struct 
        {
            ref var entityData = ref entity.World.GetCheckedEntityData(entity);

            var componentTypeIndex = ComponentType<T>.Index;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == componentTypeIndex)
                {
                    // Found component
                    entityData.Components[i].Version = entity.World.GlobalSystemVersion;

                    break;
                }
            }
        }

        public static ref T GetComponent<T>(in this Entity entity) where T : struct
        {
            return ref GetComponentWorker<T>(entity, dirtyEntity: true);
        }

        private static ref T GetComponentWorker<T>(
            Entity entity, 
            bool dirtyEntity) where T : struct
        {
            ref var entityData = ref entity.World.GetCheckedEntityData(entity);

            var componentTypeIndex = ComponentType<T>.Index;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == componentTypeIndex)
                {
                    // Found component
                    if (dirtyEntity)
                    {
                        entityData.Components[i].Version = entity.World.GlobalSystemVersion;
                    }

                    return ref ((ComponentPool<T>)entity.World.ComponentPools[componentTypeIndex]).GetItem(entityData.Components[i].ItemIndex);
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
                TypeIndex = componentTypeIndex,
                ItemIndex = index,
            };
            entityData.ComponentCount++;

            entity.World.UpdateEntityQueries(componentTypeIndex, entity, entityData, isDelete: false);

            return ref pool.GetItem(index);
        }

        public static ref readonly T GetReadOnlyComponent<T>(in this Entity entity) where T : struct
        {
            return ref GetComponentWorker<T>(entity, dirtyEntity: false);
        }

        public static bool HasComponent<T>(in this Entity entity) where T : struct
        {
            ref var entityData = ref entity.World.GetCheckedEntityData(entity);

            var componentTypeIndex = ComponentType<T>.Index;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == componentTypeIndex)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool RemoveComponent<T>(in this Entity entity) where T : struct
        {
            bool wasRemoved = false;

            var componentTypeIndex = ComponentType<T>.Index;

            ref var entityData = ref entity.World.GetCheckedEntityData(entity);

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == componentTypeIndex)
                {
                    entity.World.UpdateEntityQueries(componentTypeIndex, entity, entityData, isDelete: true);

                    entity.World.ComponentPools[componentTypeIndex].Free(entityData.Components[i].ItemIndex);

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

            var poolIndex = ComponentType<T>.Index;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].TypeIndex == poolIndex)
                {
                    return new ComponentRef<T>(
                        entity, 
                        (ComponentPool<T>)entity.World.ComponentPools[poolIndex], 
                        entityData.Components[i].ItemIndex);
                    //return 
                    //    ((ComponentPool<T>)entity.World.ComponentPools[poolIndex])
                    //    .Reference(entityData.Components[i].ItemIndex);
                }
            }

            return default;
        }

        public static void Free(in this Entity entity)
        {
            ref var entityData = ref entity.World.GetCheckedEntityData(entity);

            for (int i = entityData.ComponentCount - 1; i >= 0; i--)
            {
                var componentTypeIndex = entityData.Components[i].TypeIndex;

                entity.World.UpdateEntityQueries(componentTypeIndex, entity, entityData, isDelete: true);
                entity.World.ComponentPools[componentTypeIndex].Free(entityData.Components[i].ItemIndex);
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
                entityData.Generation != entity.Generation ||
                entityData.ComponentCount == 0;
        }
    }
}
