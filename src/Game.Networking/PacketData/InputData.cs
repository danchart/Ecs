using Common.Core;
using Game.Networking.Core;
using Game.Simulation.Core;
using System.IO;

namespace Game.Networking.PacketData
{
    public struct InputData
    {
        public const int FieldCount = 2;

        // 0
        public float x_axis;
        // 1
        public float y_axis;
        // 2
        public bool fire_down;

        public int Serialize(in BitField hasFields, Stream stream, bool measureOnly)
        {
            int size = 0;

            if (hasFields.IsSet(0))
            {
                size += stream.PacketWriteByte(0, measureOnly);
                size += stream.PacketWriteFloat(this.x_axis, measureOnly);
            }

            if (hasFields.IsSet(1))
            {
                size += stream.PacketWriteByte(1, measureOnly);
                size += stream.PacketWriteFloat(this.y_axis, measureOnly);
            }

            if (hasFields.IsSet(2))
            {
                size += stream.PacketWriteByte(2, measureOnly);
                size += stream.PacketWriteByte((byte)(this.fire_down ? 1 : 0), measureOnly);
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

                byte temp_byte;

                switch (fieldIndex)
                {
                    case 0:
                        stream.PacketReadFloat(out this.x_axis);
                        break;
                    case 1:
                        stream.PacketReadFloat(out this.y_axis);
                        break;
                    case 2:
                        stream.PacketReadByte(out temp_byte);

                        this.fire_down = temp_byte == 1 ? true : false;
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }
    }

    public static class PlayerInputDataExtensions
    {
        public static void ToPacket(
            this in InputComponent component,
            ref InputData packet)
        {
            packet.x_axis = component.Horizontal;
            packet.y_axis = component.Vertical;
            packet.fire_down = component.IsFire1Down;
        }

        public static void FromPacket(
            this in InputData packet,
            in BitField hasFields,
            ref InputComponent component)
        {
            component.Horizontal = hasFields.Bit0 ? packet.x_axis : component.Horizontal;
            component.Vertical = hasFields.Bit1 ? packet.y_axis : component.Vertical;
            component.IsFire1Down = hasFields.Bit2 ? packet.fire_down: component.IsFire1Down;
        }
    }
}