using Ecs.Core;

namespace Ecs.Simulation.Server
{
    public struct ReplicatedEntityData
    {
        public AppendOnlyList<int> Entities;
        public AppendOnlyList<ReplicatedComponentData> ComponentData;

        public ReplicatedEntityData(int entityCapacity, int componentCapacity)
        {
            Entities = new AppendOnlyList<int>(entityCapacity);
            ComponentData = new AppendOnlyList<ReplicatedComponentData>(componentCapacity);
        }
    }
}
