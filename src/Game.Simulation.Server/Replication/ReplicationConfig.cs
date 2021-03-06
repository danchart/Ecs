﻿namespace Game.Simulation.Server
{
    public sealed class ReplicationConfig
    {
        public static readonly ReplicationConfig Default = new ReplicationConfig();

        public int PhysicsHistoryCount = 10;

        public CapacityConfig Capacity = new CapacityConfig();
        public PacketPriorityConfig PacketPriority = new PacketPriorityConfig();
        public NetworkingConfig Networking = new NetworkingConfig();

        public float GridSize = 10.0f;

        public sealed class CapacityConfig
        {
            public int InitialReplicatedEntityCapacity = 1024;
            public int InitialReplicatedComponentCapacity = 6;
        }

        public sealed class PacketPriorityConfig
        {
            public float
                DistanceSquardRing0 = 12.0f,
                DistanceSquardRing1 = 24.0f,
                DistanceSquardRing2 = 48.0f;

            public float
                Ring0Priority = 1.0f,
                Ring1Priority = 0.9f,
                Ring2Priority = 0.5f,
                Ring3Priority = 0.1f;
        }

        public sealed class NetworkingConfig
        {
            /// <summary>
            /// Base tick time for priority queue. 
            /// </summary>
            public float PriorityQueueDelayBaseTick = 0.016f;

            /// <summary>
            /// Requested queue delay (in seconds) per queue priority index to queue replication packet. 
            /// 
            /// 0 is highest and Length - 1 is lowest priority.
            /// </summary>
            public int[] PriorityQueueDelay = new int[]
            {
            // 0 - Highest priority, immediate dispatch.
            0,
            // 1
            4,
            // 2
            8,
            // 3
            20
            };
        }
    }
}
