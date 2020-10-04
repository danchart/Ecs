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
                if (entityData.ComponentPoolIndices[i] == componentPoolIndex)
                {
                    // Found component
                    return ref ((ComponentPool<T>)World.ComponentPools[componentPoolIndex]).GetItem(entityData.ComponentIndices[i]);
                }
            }

            // Create component

            if (entityData.ComponentIndices.Length == entityData.ComponentCount)
            {
                Array.Resize(ref entityData.ComponentIndices, entityData.ComponentCount * 2);
                Array.Resize(ref entityData.ComponentPoolIndices, entityData.ComponentCount * 2);
            }

            var pool = World.GetPool<T>();
            var index = pool.New();
            entityData.ComponentIndices[entityData.ComponentCount] = index;
            entityData.ComponentPoolIndices[entityData.ComponentCount] = componentPoolIndex;
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