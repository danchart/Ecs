namespace Ecs.Core
{
    public static class EntityQueryExtensions
    {
        public static ref readonly T Get<T>(this EntityQuery<T> query) 
            where T : unmanaged
        {
            return ref query.GetReadonly(0);
        }
    }
}
