namespace Ecs.Core
{
    public static class EntityQueryExtensions
    {
        internal static void CopyTo(
            this ref EntityQueryBase.EntityItem source,
            ref EntityQueryBase.EntityItem destination)
        {
            // Copy allowed since the only reference type is Entity.World.
            destination = source;
        }

        internal static void CopyTo(
            this ref EntityQueryBase.PendingEntityUpdate source,
            ref EntityQueryBase.PendingEntityUpdate destination)
        {
            // Copy allowed since the only reference type is Entity.World.
            destination = source;
        }
    }
}
