using Common.Core;
using Ecs.Core;
using Game.Networking;
using Game.Simulation.Core;
using Simulation.Core;
using System;

namespace Game.Simulation.Server
{
    public class SpawnPlayerSimulationCommand : ISimulationCommand
    {
        private readonly PlayerId _playerId;
        private readonly IPhysicsWorld _physicsWorld;

        public SpawnPlayerSimulationCommand(
            PlayerId playerId, 
            IPhysicsWorld physicsWorld,
            PlayerConnectionRef connectionRef)
        {
            this._playerId = playerId;
            this._physicsWorld = physicsWorld;
        }

        public bool CanExecute(World world)
        {
            return true;
        }

        public void ExecuteAsync(World world)
        {
            // TODO: Player will need more sophisticated construction, e.g. initial location
            var playerEntity = world.NewEntity();
            // Disable from simulation until initialized.
            playerEntity.GetComponent<IsDisabledComponent>();

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

            this._players.Add(
                in connectionRef,
                playerEntity);

            // Entity is ready for the simulation.
            playerEntity.RemoveComponent<IsDisabledComponent>();

        }
    }
}
