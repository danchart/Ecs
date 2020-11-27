using Common.Core;
using Networking.Core;
using Game.Simulation.Core;
using System.IO;

namespace Game.Networking.PacketData
{
    public struct MovementData
    {
        public const int FieldCount = 2;

        // 0
        public float velocity_x;
        // 1
        public float velocity_y;

        public int Serialize(BitField hasFields, Stream stream)
        {
            int size = 0;

            if (hasFields.IsSet(0))
            {
                size += stream.PacketWriteByte(0);
                size += stream.PacketWriteFloat(velocity_x);
            }

            if (hasFields.IsSet(1))
            {
                size += stream.PacketWriteByte(1);
                size += stream.PacketWriteFloat(velocity_y);
            }

            return size;
        }

        public bool Deserialize(byte fieldCount, ref BitField hasFields, Stream stream)
        {
            for (int i = 0; i < fieldCount; i++)
            {
                byte fieldIndex;
                stream.PacketReadByte(out fieldIndex);

                hasFields.Set(fieldIndex);

                switch (fieldIndex)
                {
                    case 0:
                        stream.PacketReadFloat(out velocity_x);
                        break;
                    case 1:
                        stream.PacketReadFloat(out velocity_y);
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }
    }

    public static class MovementDataExtensions
    {
        public static void ToPacket(
            this in MovementComponent component, 
            ref MovementData packet)
        {
            packet.velocity_x = component.velocity.x;
            packet.velocity_y = component.velocity.y;
        }

        public static void FromPacket(
            this in MovementData packet,
            in BitField hasFields,
            ref MovementComponent component)
        {
            component.velocity.x = hasFields.Bit0 ? packet.velocity_x : component.velocity.x;
            component.velocity.y = hasFields.Bit1 ? packet.velocity_y : component.velocity.y;
        }

        public static void Merge(
            this MovementData data,
            in MovementData newData,
            ref BitField hasFields)
        {
            // 0/velocity_x
            if (data.velocity_x != newData.velocity_x)
            {
                data.velocity_x = newData.velocity_x;
                hasFields.Set(0);
            }

            // 1/velocity_y
            if (data.velocity_y != newData.velocity_y)
            {
                data.velocity_y = newData.velocity_y;
                hasFields.Set(1);
            }
        }
    }
}
