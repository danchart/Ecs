using Common.Core;
using Game.Networking.PacketData;
using Game.Simulation.Core;
using System;
using System.Runtime.InteropServices;

namespace Game.Simulation.Server
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ReplicatedComponentData
    {
        [FieldOffset(0)]
        public ComponentId ComponentId;

        [FieldOffset(2)]
        public TransformData Transform;
        [FieldOffset(2)]
        public MovementData Movement;
        [FieldOffset(2)]
        public PlayerData Player;

        public int ComponentIdAsIndex => (int)ComponentId;
    }

    public static class ReplicatedComponentDataExtensions
    {
        public static void Merge(
            this ReplicatedComponentData data,
            in ReplicatedComponentData newData,
            ref BitField hasFields)
        {
            switch (data.ComponentId)
            {
                case ComponentId.Transform:
                    data.Transform.Merge(newData.Transform, ref hasFields);
                    break;
                case ComponentId.Movement:
                    data.Movement.Merge(newData.Movement, ref hasFields);
                    break;
                default:
                    // You forgot to update Merge()
                    throw new InvalidOperationException($"Unknown ComponentId: {data.ComponentId}");
            }
        }
    }
}
