using Ecs.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;
using System.Runtime.InteropServices;

namespace Game.Simulation.Server
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ReplicatedComponentData
    {
        [FieldOffset(0)]
        public int ComponentId;

        [FieldOffset(2)]
        public TransformData Transform;
        [FieldOffset(2)]
        public MovementData Movement;

        //[FieldOffset(2)]
        //public ComponentRef<TransformComponent> Transform;
        //[FieldOffset(2)]
        //public ComponentRef<MovementComponent> Movement;
    }
}
