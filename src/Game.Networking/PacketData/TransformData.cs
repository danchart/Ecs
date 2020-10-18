using Common.Core;
using Game.Networking.Core;
using Game.Simulation.Core;
using System.IO;
using System.Runtime.CompilerServices;

namespace Game.Networking.PacketData
{
    public struct TransformData
    {
        public const int FieldCount = 3;

        // 0
        public float x;
        // 1
        public float y;
        // 2
        public float rotation;

        public bool Serialize(in BitField hasFields, Stream stream)
        {
            if (hasFields.IsSet(0))
            {
                stream.PacketWriteByte(0);
                stream.PacketWriteFloat(x);
            }

            if (hasFields.IsSet(1))
            {
                stream.PacketWriteByte(1);
                stream.PacketWriteFloat(y);
            }

            if (hasFields.IsSet(2))
            {
                stream.PacketWriteByte(2);
                stream.PacketWriteFloat(rotation);
            }

            return true;
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
                        stream.PacketReadFloat(out x);
                        break;
                    case 1:
                        stream.PacketReadFloat(out y);
                        break;
                    case 2:
                        stream.PacketReadFloat(out rotation);
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }
    }

    public static class TransformDataExtensions
    {
        public static void ToPacket(
            this in TransformComponent component, 
            ref TransformData packet)
        {
            packet.x = component.position.x;
            packet.y = component.position.y;
            packet.rotation = component.rotation;
        }

        public static void FromPacket(
            this in TransformData packet, 
            in BitField hasFields, 
            ref TransformComponent component)
        {
            component.position.x = hasFields.Bit0 ? packet.x : component.position.x;
            component.position.y = hasFields.Bit1 ? packet.y : component.position.y;
            component.rotation = hasFields.Bit2 ? packet.rotation : component.rotation;
        }

        public static void Merge(
            this TransformData data,
            in TransformData newData,
            ref BitField hasFields)
        {
            // 0/x
            if (data.x != newData.x)
            {
                data.x = newData.x;
                hasFields.Set(0);
            }

            // 1/y
            if (data.y != newData.y)
            {
                data.y = newData.y;
                hasFields.Set(1);
            }

            // 2/rotation
            if (data.rotation != newData.rotation)
            {
                data.rotation = newData.rotation;
                hasFields.Set(2);
            }
        }
    }
}
