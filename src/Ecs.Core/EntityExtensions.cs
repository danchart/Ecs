namespace Ecs.Core
{
    public static class EntityExtensions
    {
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
    }
}
