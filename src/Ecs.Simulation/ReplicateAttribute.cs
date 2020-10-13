using System;

namespace Ecs.Simulation
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class ReplicateAttribute : Attribute
    {
        public ReplicateAttribute()
        {
        }
    }
}
