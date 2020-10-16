using Ecs.Core;

namespace Game.Simulation.Client
{
    internal struct SnapShot<TInput>
    {
        public AppendOnlyList<ClientInputFrame> ClientInputs;
        public WorldState WorldState;
        public float Time;

        public void Reset()
        {
            ClientInputs.Resize(0);
        }

        public void MoveInputsTo(ref AppendOnlyList<ClientInputFrame> inputs)
        {
            this.ClientInputs.ShallowCopyTo(inputs);

            this.ClientInputs.Count = 0;
        }

        public struct ClientInputFrame
        {
            public float Time;
            public TInput Input;
        }
    }
}
