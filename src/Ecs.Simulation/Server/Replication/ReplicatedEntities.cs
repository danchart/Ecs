using Ecs.Core;

namespace Ecs.Simulation.Server
{
    public class ReplicatedEntities
    {
        private readonly EntityListValueCollection<AppendOnlyList<ReplicatedComponentData>> EntityComponents;

        private readonly int _componentCapacity;

        public ReplicatedEntities(int entityCapacity, int componentCapacity)
        {
            _componentCapacity = componentCapacity;
                 
            EntityComponents = new EntityListValueCollection<AppendOnlyList<ReplicatedComponentData>>(entityCapacity);
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
