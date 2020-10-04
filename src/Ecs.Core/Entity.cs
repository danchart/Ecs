using Ecs.Core.src.Hashing;
using System;

namespace Ecs.Core
{
    public struct Entity : IEquatable<Entity>
    {
        public World World;
        public int Id;

        public ref T GetComponent<T>() where T : struct
        {
            ref var entityData = ref World.GetEntityData(this);

            var componentPoolIndex = ComponentType<T>.ComponentPoolIndex;

            for (int i = 0; i < entityData.ComponentCount; i++)
            {
                if (entityData.Components[i].PoolIndex == componentPoolIndex)
                {
                    // Found component
                    return ref ((ComponentPool<T>)World.ComponentPools[componentPoolIndex]).GetItem(entityData.Components[i].ItemIndex);
                }
            }

            // Create component

            if (entityData.Components.Length == entityData.ComponentCount)
            {
                Array.Resize(ref entityData.Components, entityData.ComponentCount * 2);
            }

            var pool = World.GetPool<T>();
            var index = pool.New();
            entityData.Components[entityData.ComponentCount] = new World.EntityData.ComponentData
            {
                PoolIndex = componentPoolIndex,
                ItemIndex = index
            };
            entityData.ComponentCount++;

            return ref pool.GetItem(index);
        }

        public static bool operator ==(in Entity lhs, in Entity rhs)
        {
            return lhs.Id == rhs.Id;
        }

        public static bool operator !=(in Entity lhs, in Entity rhs)
        {
            return lhs.Id != rhs.Id;
        }

        public override int GetHashCode()
        {
            return
                HashingUtil.CombineHashCodes(
                    Id,
                    World.GetHashCode());
        }

        public override bool Equals(object other)
        {
            return other is Entity otherEntity && Equals(otherEntity);
        }

        public bool Equals(Entity other)
        {
            return Id == other.Id && World == other.World;
        }
    }
}