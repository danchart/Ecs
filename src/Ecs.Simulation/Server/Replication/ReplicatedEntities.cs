using Ecs.Core;

namespace Ecs.Simulation.Server
{
    public class ReplicatedEntities
    {
        private readonly EntityCollection<AppendOnlyList<ReplicatedComponentData>> EntityComponents;

        private readonly int _componentCapacity;

        public ReplicatedEntities(int entityCapacity, int componentCapacity)
        {
            _componentCapacity = componentCapacity;
                 
            EntityComponents = new EntityCollection<AppendOnlyList<ReplicatedComponentData>>(entityCapacity);
        }

        public void Clear()
        {
            foreach (var value in EntityComponents)
            {
                value.Clear();
            }

            EntityComponents.Clear();
        }

        public AppendOnlyList<ReplicatedComponentData> this[Entity entity]
        {
            get
            {
                if (!EntityComponents.Contains(entity))
                {
                    EntityComponents[entity] = new AppendOnlyList<ReplicatedComponentData>(_componentCapacity);
                }

                return EntityComponents[entity];
            }
        }
    }
}
