using Game.Networking.PacketData;
using Game.Simulation.Core;
using System.Runtime.InteropServices;

namespace Game.Simulation.Server
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ReplicatedComponentData
    {
        [FieldOffset(0)]
        public ComponentId ComponentId;
        [FieldOffset(2)]
        public ushort FieldCount;

        [FieldOffset(4)]
        public TransformData Transform;
        [FieldOffset(4)]
        public MovementData Movement;

        public int ComponentIdAsIndex => (int)ComponentId;
    }
}
