using Common.Core;
using Game.Networking.Core;
using Game.Networking.PacketData;
using System.IO;

namespace Game.Networking
{
    public struct ClientInputPacket
    {
        /// <summary>
        /// Clients frame index for this input.
        /// </summary>
        public FrameIndex Frame;

        /// <summary>
        /// Last fully received server frame index.
        /// </summary>
        public FrameIndex LastServerFrame;

        /// <summary>
        /// Count of inputs.
        /// </summary>
        public byte InputCount;

        /// <summary>
        /// Inputs. Expected frame order is [0] is Frame - Inputs.Length, [Inputs.Length-1] is Frame.
        /// </summary>
        public InputPacketData[] Inputs;

        public int Serialize(Stream stream, bool measureOnly)
        {
            // frame index
            int size = stream.PacketWriteUShort(Frame, measureOnly);
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
            // frame index
            ushort frameIndexAsUshort;
            stream.PacketReadUShort(out frameIndexAsUshort);
            this.Frame = new FrameIndex(frameIndexAsUshort);
            // input count
            stream.PacketReadByte(out InputCount);

            Inputs = new InputPacketData[InputCount];

            for (int i = 0; i < InputCount; i++)
            {
                Inputs[i].Deserialize(stream);
            }

            return true;
        }
    }

    public struct InputPacketData
    {
        public BitField HasFields;

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
