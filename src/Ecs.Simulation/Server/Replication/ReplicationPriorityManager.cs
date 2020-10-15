using Ecs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecs.Simulation.Server
{
    public interface IReplicationPriorityManager
    {
        ReplicationPriority GetPriority(Entity player, )
    }

    public class ReplicationPriorityManager : IReplicationPriorityManager
    {

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
