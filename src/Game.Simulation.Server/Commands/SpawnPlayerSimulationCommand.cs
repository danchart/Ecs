﻿using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using Simulation.Core;

namespace Game.Simulation.Server
{
    public class SpawnPlayerSimulationCommand : ISimulationIngressCommand
    {
        private readonly WorldPlayers _players;
        private readonly PlayerConnectionRef _connectionRef;
        private readonly PlayerId _playerId;
        private readonly IPhysicsWorld _physicsWorld;

        public SpawnPlayerSimulationCommand(
            WorldPlayers players,
            PlayerId playerId, 
            IPhysicsWorld physicsWorld,
            PlayerConnectionRef connectionRef)
        {
            this._players = players;
            this._connectionRef = connectionRef;
            this._playerId = playerId;
            this._physicsWorld = physicsWorld;
        }

        public bool CanExecute(World world) => true;

        public void Execute(World world, ISimulationEgressCommands egressCommands)
        {
            // TODO: Player will need more sophisticated construction, e.g. initial location
            var playerEntity = world.NewEntity();

            ref var playerComponent = ref playerEntity.GetComponent<PlayerComponent>();
            playerComponent.Id = this._playerId;
            playerEntity.GetComponent<RigidBodyComponent>();
            ref var transform = ref playerEntity.GetComponent<TransformComponent>();
            playerEntity.GetComponent<MovementComponent>();

            this._physicsWorld.AddCircle(
                playerEntity,
                isStatic: false,
                originWS: transform.position,
                rotation: 0,
                radius: 0.5f);

            egressCommands.AddEgress(new AddConnectionEgressCommand(
                this._players,
                this._connectionRef,
                playerEntity));
        }

        private class AddConnectionEgressCommand : ISimulationEgressCommand
        {
            readonly WorldPlayers _players;
            readonly PlayerConnectionRef _connectionRef;
            readonly Entity _playerEntity;

            public AddConnectionEgressCommand(WorldPlayers players, PlayerConnectionRef connectionRef, Entity playerEntity)
            {
                _players = players;
                _connectionRef = connectionRef;
                _playerEntity = playerEntity;
            }

            public bool CanExecute(World world) => true;

            public void Execute(World world)
            {
                this._players.Add(_connectionRef, _playerEntity);
            }
        }
    }
}
