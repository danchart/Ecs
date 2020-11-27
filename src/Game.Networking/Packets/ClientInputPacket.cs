using Common.Core;
using Networking.Core;
using Game.Networking.PacketData;
using System.IO;

namespace Game.Networking
{
    public struct ClientInputPacket
    {
        public PacketHeader Header;

        /// <summary>
        /// Count of inputs.
        /// </summary>
        public byte InputCount;

        /// <summary>
        /// Inputs. Expected frame order is [0] is Frame - Inputs.Length, [Inputs.Length-1] is Frame.
        /// </summary>
        public InputPacketData[] Inputs;

        public int Serialize(Stream stream)
        {
            // last frame ack 
            int size = Header.Serialize(stream);
            // entity count
            size += stream.PacketWriteByte(InputCount);

            for (int i = 0; i < InputCount; i++)
            {
                size += Inputs[i].Serialize(stream);
            }

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // header
            Header.Deserialize(stream);

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

        public InputData Input;

        public int Serialize(Stream stream)
        {
            byte fieldCount = (byte)HasFields.Count();
            int size = stream.PacketWriteByte(fieldCount);
            size += Input.Serialize(HasFields, stream);

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
