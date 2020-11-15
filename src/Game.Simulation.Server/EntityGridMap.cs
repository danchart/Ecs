using Common.Core.Numerics;
using Ecs.Core;
using System;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public interface IEntityGridMap
    {
        void AddOrUpdate(in Entity entity, in Vector2 position);
        bool Remove(in Entity entity);

        HashSet<Entity> GetEntities(int row, int column);

        void GetGridPosition(in Vector2 position, out int row, out int column);

        void GetEntitiesOfInterest(in Entity entity, ref Entity[] entitiesOfInterest, out int count);
    }

    public class EntityGridMap : IEntityGridMap
    {
        private readonly Dictionary<int, HashSet<Entity>> _hashToEntities;
        private readonly Dictionary<Entity, int> _entityToHash;

        private readonly float InverseGridSize;
        private readonly int CellCapacity;

        public EntityGridMap(
            float gridSize, 
            int gridCapacity = 1024, 
            int entityCapacity = 1024, 
            int cellCapacity = 256)
        {
            this._hashToEntities = new Dictionary<int, HashSet<Entity>>(gridCapacity);
            this._entityToHash = new Dictionary<Entity, int>(entityCapacity);
            this.InverseGridSize = 1.0f / gridSize;
            this.CellCapacity = cellCapacity;
        }

        public void AddOrUpdate(in Entity entity, in Vector2 position)
        {
            GetGridPosition(position, out int row, out int column);

            var gridHash = GetGridHashFromPosition(row, column);

            if (this._entityToHash.ContainsKey(entity))
            {
                // Entity has previously been hashed.

                if (this._entityToHash[entity] == gridHash)
                {
                    // The entity is already at the correct hash position.
                    return;
                }

                Remove(entity);
            }

            if (!this._hashToEntities.ContainsKey(gridHash))
            {
                this._hashToEntities[gridHash] = new HashSet<Entity>(this.CellCapacity);
            }

            this._hashToEntities[gridHash].Add(entity);
            this._entityToHash[entity] = gridHash;
        }

        public bool Remove(in Entity entity)
        {
            return
                this._hashToEntities[this._entityToHash[entity]].Remove(entity) ||
                this._entityToHash.Remove(entity);
        }

        public HashSet<Entity> GetEntities(int row, int column)
        {
            var gridHash = GetGridHashFromPosition(row, column);

            return this._hashToEntities[gridHash];
        }

        public void GetGridPosition(in Vector2 position, out int row, out int column)
        {
            row = (int) (position.x * this.InverseGridSize);
            column = (int) (position.y *  this.InverseGridSize);
        }

        public void GetEntitiesOfInterest(in Entity entity, ref Entity[] entitiesOfInterest, out int count)
        {
            GetGridPositionFromHash(this._entityToHash[entity], out int row, out int column);

            count = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    var entities = GetEntities(row + i, column + j);

                    if (count + entities.Count > entitiesOfInterest.Length)
                    {
                        Array.Resize(ref entitiesOfInterest, 2 * (count + entities.Count));
                    }

                    entities.CopyTo(entitiesOfInterest, count);

                    count += entities.Count;
                }
            }
        }

        private static int GetGridHashFromPosition(int row, int column)
        {
            return (column << 16) | (row & 0xffff);
        }

        private static void GetGridPositionFromHash(int hash, out int row, out int column)
        {
            row = (hash & 0xffff);
            column = (hash >> 16);
        }
    }
}
