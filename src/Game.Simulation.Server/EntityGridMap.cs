using Common.Core.Numerics;
using Ecs.Core;
using System.Collections.Generic;

namespace Game.Simulation.Server
{
    public interface IEntityGridMap
    {
        void AddOrUpdate(Entity entity, in Vector2 position);
        bool Remove(Entity entity);

        HashSet<Entity> GetEntities(int row, int column);

        void GetGridPosition(in Vector2 position, out int row, out int column);
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

        public void AddOrUpdate(Entity entity, in Vector2 position)
        {
            GetGridPosition(position, out int row, out int column);

            var gridHash = GetGridHash(row, column);

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

        public bool Remove(Entity entity)
        {
            return
                this._hashToEntities[this._entityToHash[entity]].Remove(entity) ||
                this._entityToHash.Remove(entity);
        }

        public HashSet<Entity> GetEntities(int row, int column)
        {
            var gridHash = GetGridHash(row, column);

            return this._hashToEntities[gridHash];
        }

        public void GetGridPosition(in Vector2 position, out int row, out int column)
        {
            row = (int) (position.x * this.InverseGridSize);
            column = (int) (position.y *  this.InverseGridSize);
        }

        private static int GetGridHash(int row, int column)
        {
            return (column << 16) | (row & 0xffff);
        }
    }
}
