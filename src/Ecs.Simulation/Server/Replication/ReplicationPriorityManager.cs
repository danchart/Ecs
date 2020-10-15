using Ecs.Core;

namespace Ecs.Simulation.Server
{
    public interface IReplicationPriorityManager
    {
        ReplicationPriority[] GetPriorities(Entity player, ReplicatedEntities replicatedEntities);
    }

    public class ReplicationPriorityManager : IReplicationPriorityManager
    {
        public ReplicationPriority[] GetPriorities(Entity player, ReplicatedEntities replicatedEntities)
        {
            ref readonly var position = ref player.GetReadOnlyComponent<TransformComponent>();

            foreach (var components in replicatedEntities)
            {

            }
        }
    }

    public struct ReplicationPriority
    {
        /// <summary>
        /// The final computed priority. [0..1]
        /// </summary>
        public float FinalPriority;

        /// <summary>
        /// The player perceived relevance or visibility. [0..1]
        /// </summary>
        public float Relevance;

        /// <summary>
        /// Desired update period, in milliseconds.
        /// </summary>
        public float UpdatePeriod;
    }
}
