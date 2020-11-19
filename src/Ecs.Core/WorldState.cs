using System;
using static Ecs.Core.World;

namespace Ecs.Core
{
    public struct WorldState
    {
        internal Version GlobalVersion;

        internal IComponentPool[] ComponentPools;

        internal EntityData[] _entities;
        internal int[] _freeEntityIds;

        internal int _entityCount;
        internal int _freeEntityCount;

        internal AppendOnlyList<EntityQueryBase> _queries;

        // Per-systems 
        internal AppendOnlyList<Version> LastSystemVersion;
    }

    public static class WorldStateExtensions
    {
        /// <summary>
        /// Copies state from one World reference to another.
        /// </summary>
        public static WorldState CopyStateTo(this in WorldState source, ref WorldState copiedState)
        {
            copiedState.GlobalVersion = source.GlobalVersion;
            copiedState.LastSystemVersion = source.LastSystemVersion;

            copiedState._freeEntityIds = source._freeEntityIds;
            copiedState._entityCount = source._entityCount;
            copiedState._freeEntityCount = source._freeEntityCount;

            if (copiedState.ComponentPools == null)
            {
                copiedState.ComponentPools = new IComponentPool[source.ComponentPools.Length];

                for (int i = 0; i < ComponentPool.PoolCount; i++)
                {
                    // Must check null since ComponentPool.PoolCount is for all components in the app domain.
                    if (source.ComponentPools[i] != null)
                    {
                        copiedState.ComponentPools[i] = source.ComponentPools[i].Clone();
                    }
                }
            }
            else
            {
                for (int i = 0; i < ComponentPool.PoolCount; i++)
                {
                    // Must check null since ComponentPool.PoolCount is for all components in the app domain.
                    if (source.ComponentPools[i] != null)
                    {
                        source.ComponentPools[i].CopyTo(copiedState.ComponentPools[i]);
                    }
                }
            }

            //copiedState.ComponentPools = new IComponentPool[source.ComponentPools.Length];
            //Array.Copy(source.ComponentPools, copiedState.ComponentPools, source.ComponentPools.Length);

            //for (int i = 0; i < ComponentPool.PoolCount; i++)
            //{
            //    source.ComponentPools[i].CopyTo(copiedState.ComponentPools[i]);
            //}

            copiedState._entities = new EntityData[source._entities.Length];
            Array.Copy(source._entities, copiedState._entities, source._entityCount);

            for (int i = 0; i < source._entityCount; i++)
            {
                source._entities[i].CopyTo(ref copiedState._entities[i]);
            }

            copiedState._freeEntityIds = new int[source._freeEntityIds.Length];
            Array.Copy(source._freeEntityIds, copiedState._freeEntityIds, source._freeEntityCount);

            // Queries are harder...

            if (copiedState._queries == null)
            {
                // Allocate and copy list

                copiedState._queries = new AppendOnlyList<EntityQueryBase>(source._queries.Items.Length);

                for (int i = 0; i < source._queries.Count; i++)
                {
                    copiedState._queries.Add(source._queries.Items[i].Clone());
                }
            }
            else
            {
                // Resize and copy list

                copiedState._queries.Resize(source._queries.Count);

                // Copy list
                for (int i = 0; i < source._queries.Count; i++)
                {
                    if (copiedState._queries.Items[i] == null)
                    {
                        // ### Heap alloc
                        copiedState._queries.Items[i] = source._queries.Items[i].Clone();
                    }

                    source._queries.Items[i].CopyTo(copiedState._queries.Items[i]);
                }
            }

            copiedState.LastSystemVersion = copiedState.LastSystemVersion ?? new AppendOnlyList<Version>(source.LastSystemVersion.Count);
            source.LastSystemVersion.ShallowCopyTo(copiedState.LastSystemVersion);

            return copiedState;
        }
    }
}
