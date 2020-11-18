using Ecs.Core;
using Game.Networking;
using System;

namespace Game.Simulation.Server
{
    public struct WorldPlayer
    {
        public PlayerConnectionRef ConnectionRef;

        public Entity Entity;

        internal int PlayerReplicationDataPoolIndex;
        internal int PlayerInputsPoolIndex;

        private bool _isConstructed;

        private readonly PlayerReplicationDataPool _replicationDataPool;
        private readonly PlayerInputsPool _playerInputsPool;

        internal WorldPlayer(
            PlayerReplicationDataPool replicationDataPool, 
            PlayerInputsPool playerInputsPool) 
            : this()
        {
            this._replicationDataPool = replicationDataPool ?? throw new ArgumentNullException(nameof(replicationDataPool));
            this._playerInputsPool = playerInputsPool ?? throw new ArgumentNullException(nameof(playerInputsPool));
            this._isConstructed = true;
        }

        public bool IsConstructed => this._isConstructed;

        public PlayerInputs PlayerInputs
        {
            get => this._playerInputsPool.Get(PlayerInputsPoolIndex);
        }

        public PlayerReplicationData ReplicationData
        {
            get => this._replicationDataPool.GetItem(PlayerReplicationDataPoolIndex);
        }
    }
}
