using Game.Networking.Core;
using System.IO;

namespace Game.Networking.PacketData
{
    public struct MovementData
    {
        // 0
        public float velocity_x;
        // 1
        public float velocity_y;

        public bool Serialize(BitField hasFields, Stream stream)
        {
            if (hasFields.IsSet(0))
            {
                stream.PacketWriteByte(0);
                stream.PacketWriteFloat(velocity_x);
            }

            if (hasFields.IsSet(1))
            {
                stream.PacketWriteByte(1);
                stream.PacketWriteFloat(velocity_y);
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
}
