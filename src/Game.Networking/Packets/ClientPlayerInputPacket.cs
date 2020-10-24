using Common.Core;
using Game.Networking.Core;
using Game.Networking.PacketData;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public struct ClientPlayerInputPacket
    {
        public uint Frame;
        public byte InputCount;
        public InputPacketData[] Inputs;

        public int Serialize(Stream stream, bool measureOnly)
        {
            // frame #
            int size = stream.PacketWriteUInt(Frame, measureOnly);
            // entity count
            size += stream.PacketWriteByte(InputCount, measureOnly);

            for (int i = 0; i < InputCount; i++)
            {
                size += Inputs[i].Serialize(stream, measureOnly);
            }

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // frame #
            stream.PacketReadUInt(out Frame);
            // packet count
            stream.PacketReadByte(out InputCount);

            Inputs = new InputPacketData[InputCount];

            for (int i = 0; i < InputCount; i++)
            {
                Inputs[i].Deserialize(stream);
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InputPacketData
    {
        [FieldOffset(0)]
        public BitField HasFields;

        [FieldOffset(4)]
        public PlayerInputData Input;

        public int Serialize(Stream stream, bool measureOnly)
        {
            byte fieldCount = (byte)HasFields.Count();
            int size = stream.PacketWriteByte(fieldCount, measureOnly);
            size += Input.Serialize(HasFields, stream, measureOnly);

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            HasFields = new BitField();

            byte fieldCount;
            stream.PacketReadByte(out fieldCount);

            return Input.Deserialize(fieldCount, ref HasFields, stream);
        }
    }
}
