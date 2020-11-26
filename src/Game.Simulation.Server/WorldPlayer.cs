using Ecs.Core;
using Game.Networking;
using System;

namespace Game.Simulation.Server
{
    public struct WorldPlayer
    {
        public FrameIndex Frame;
        public FrameIndex LastInputFrame;
        public FrameIndex LastAcknowledgedSimulationFrame;

        public readonly PlayerConnectionRef ConnectionRef;

        private bool _isReplicating;

        private Entity _entity;
        private int _playerReplicationDataPoolIndex;
        private int _playerInputsPoolIndex;

        private readonly PlayerReplicationDataPool _replicationDataPool;
        private readonly PlayerInputsPool _playerInputsPool;

        internal WorldPlayer(
            PlayerReplicationDataPool replicationDataPool, 
            PlayerInputsPool playerInputsPool,
            in PlayerConnectionRef connectionRef) 
            : this()
        {
            this._replicationDataPool = replicationDataPool ?? throw new ArgumentNullException(nameof(replicationDataPool));
            this._playerInputsPool = playerInputsPool ?? throw new ArgumentNullException(nameof(playerInputsPool));

            this._playerReplicationDataPoolIndex = this._replicationDataPool.New();
            this._playerInputsPoolIndex = this._playerInputsPool.New();

            this.ConnectionRef = connectionRef;

            this._isReplicating = false;
        }

        public PlayerInputs PlayerInputs
        {
            get => this._playerInputsPool.Get(_playerInputsPoolIndex);
        }

        public PlayerReplicationData ReplicationData
        {
            get => this._replicationDataPool.GetItem(_playerReplicationDataPoolIndex);
        }

        public void StartReplication(in Entity entity)
        {
            this._entity = entity;

            this.Frame = FrameIndex.Zero;
            this.LastAcknowledgedSimulationFrame = FrameIndex.Zero;
            this.LastInputFrame = FrameIndex.Zero;

            this._isReplicating = true;
        }

        public bool TryGetEntity(out Entity entity)
        {
            entity = this._entity;

            return this._isReplicating;
        }

        public void Free()
        {
            this._replicationDataPool.Free(this._playerReplicationDataPoolIndex);
            this._playerInputsPool.Free(this._playerInputsPoolIndex);
            this._entity.Free();

            this._isReplicating = false;
        }
    }
}
