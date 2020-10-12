using System;
using static Ecs.Core.World;

namespace Ecs.Core
{
    public struct WorldState
    {
        internal Version GlobalSystemVersion;

        internal IComponentPool[] ComponentPools;

        internal EntityData[] _entities;
        internal int[] _freeEntityIds;

        internal int _entityCount;
        internal int _freeEntityCount;

        internal AppendOnlyList<GlobalEntityQuery> _globalQueries;

        // Per-systems 
        internal AppendOnlyList<Version> LastSystemVersion;
        internal AppendOnlyList<AppendOnlyList<PerSystemsEntityQuery>> _perSystemsQueries;
    }

    public static class WorldStateExtensions
    {
        /// <summary>
        /// Copies state from one World reference to another.
        /// </summary>
        public static WorldState CopyState(this in WorldState source, ref WorldState copiedState)
        {
            copiedState.GlobalSystemVersion = source.GlobalSystemVersion;
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

            if (copiedState._globalQueries == null)
            {
                // Allocate and copy list

                copiedState._globalQueries = new AppendOnlyList<GlobalEntityQuery>(source._globalQueries.Items.Length);

                for (int i = 0; i < source._globalQueries.Count; i++)
                {
                    copiedState._globalQueries.Add((GlobalEntityQuery)source._globalQueries.Items[i].Clone());
                }
            }
            else
            {
                // Resize and copy list

                copiedState._globalQueries.Resize(source._globalQueries.Count);

                // Copy list
                for (int i = 0; i < source._globalQueries.Count; i++)
                {
                    if (copiedState._globalQueries.Items[i] == null)
                    {
                        // ### Heap alloc
                        copiedState._globalQueries.Items[i] = (GlobalEntityQuery)source._globalQueries.Items[i].Clone();
                    }

                    source._globalQueries.Items[i].CopyTo(copiedState._globalQueries.Items[i]);
                }
            }

            copiedState.LastSystemVersion = copiedState.LastSystemVersion ?? new AppendOnlyList<Version>(source.LastSystemVersion.Count);
            source.LastSystemVersion.ShallowCopyTo(copiedState.LastSystemVersion);

            // Allocate and size systems list - this is fixed after Init();
            if (copiedState._perSystemsQueries == null)
            {
                copiedState._perSystemsQueries = new AppendOnlyList<AppendOnlyList<PerSystemsEntityQuery>>(source._perSystemsQueries.Count);

                // Copy list
                for (int i = 0; i < source._perSystemsQueries.Count; i++)
                {
                    copiedState._perSystemsQueries.Add(new AppendOnlyList<PerSystemsEntityQuery>(source._perSystemsQueries.Items[i].Count));

                    // Clone list

                    for (int j = 0; j < source._perSystemsQueries.Items[i].Count; j++)
                    {
                        copiedState._perSystemsQueries.Items[i].Add((PerSystemsEntityQuery)source._perSystemsQueries.Items[i].Items[j]);
                    }
                }
            }
            else
            {
                // Copy per system query list
                for (int i = 0; i < source._perSystemsQueries.Count; i++)
                {
                    var queryList = source._perSystemsQueries.Items[i];
                    var copiedQueryList = copiedState._perSystemsQueries.Items[i];

                    copiedQueryList.Resize(queryList.Count);

                    // Copy list
                    for (int j = 0; j < queryList.Count; j++)
                    {
                        queryList.Items[j].CopyTo(copiedQueryList.Items[j]);
                    }
                }
            }

            return copiedState;
        }
    }
}
