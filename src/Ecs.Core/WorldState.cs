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

        internal AppendOnlyList<SharedEntityQuery> _sharedQueries;

        // Per-systems 
        internal AppendOnlyList<Version> LastSystemVersion;
        internal AppendOnlyList<AppendOnlyList<PerSystemsEntityQuery>> _perSystemsQueries;
    }

    public static class WorldStateExtensions
    {
        public static WorldState CopyState(this in WorldState state)
        {
            var copiedState = new WorldState
            {
                GlobalSystemVersion = state.GlobalSystemVersion,
                LastSystemVersion = state.LastSystemVersion,

                _freeEntityIds = state._freeEntityIds,
                _entityCount = state._entityCount,
                _freeEntityCount = state._freeEntityCount,
            };

            copiedState.ComponentPools = new IComponentPool[state.ComponentPools.Length];
            Array.Copy(state.ComponentPools, copiedState.ComponentPools, state.ComponentPools.Length);

            for (int i = 0; i < state.ComponentPools.Length; i++)
            {
                state.ComponentPools[i].CopyTo(copiedState.ComponentPools[i]);
            }

            copiedState._entities = new EntityData[state._entities.Length];
            Array.Copy(state._entities, copiedState._entities, state._entityCount);

            for (int i = 0; i < state._entities.Length; i++)
            {
                state._entities[i].CopyTo(ref copiedState._entities[i]);
            }

            copiedState._freeEntityIds = new int[state._freeEntityIds.Length];
            Array.Copy(state._freeEntityIds, copiedState._freeEntityIds, state._freeEntityCount);

            copiedState._sharedQueries = new AppendOnlyList<SharedEntityQuery>(state._sharedQueries.Count);
            Array.Copy(state._sharedQueries.Items, copiedState._sharedQueries.Items, copiedState._sharedQueries.Count);

            copiedState.LastSystemVersion = new AppendOnlyList<Version>(state.LastSystemVersion.Count);
            Array.Copy(state.LastSystemVersion.Items, copiedState.LastSystemVersion.Items, copiedState.LastSystemVersion.Count);

            for (int i = 0; i < state._sharedQueries.Count; i++)
            {
                copiedState._sharedQueries.Items[i] = state._sharedQueries.Items[i];

                // TODO: CopyTo()
            }

            for (int i = 0; i < state._perSystemsQueries.Count; i++)
            {
                copiedState._perSystemsQueries.Items[i] = state._perSystemsQueries.Items[i];

                // TODO: CopyTo()
            }

            return copiedState;
        }
    }
}
