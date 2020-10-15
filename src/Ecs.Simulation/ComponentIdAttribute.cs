using System;

namespace Ecs.Simulation
{
    /// <summary>
    /// Replication ID
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class ComponentIdAttribute : Attribute
    {
        public readonly ComponentId Id;

        public ComponentIdAttribute(ComponentId id)
        {
            Id = id;
        }
    }
}
