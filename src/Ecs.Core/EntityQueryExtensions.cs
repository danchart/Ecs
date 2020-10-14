namespace Ecs.Core
{
    public static class EntityQueryExtensions
    {
        internal static void CopyTo(
            this in EntityQueryBase.PendingEntityUpdate source,
            ref EntityQueryBase.PendingEntityUpdate destination)
        {
            // Copy allowed since the only reference type is Entity.World.
            destination = source;
        }
    }
}
