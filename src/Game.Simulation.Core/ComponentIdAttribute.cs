using System;

namespace Game.Simulation.Core
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
