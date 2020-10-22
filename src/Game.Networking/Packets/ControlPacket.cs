using Game.Networking.Core;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public enum ControlMessageEnum : ushort
    {
        // Imitation TCP 
        ConnectSyn,
        ConnectSynAck,
        ConnectAck
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ControlPacket
    {
        [FieldOffset(0)]
        public ControlMessageEnum ControlMessage;

        [FieldOffset(2)]
        public ControlSynPacketData ControlSynPacketData;
        [FieldOffset(2)]
        public ControlAckPacketData ControlAckPacketData;

        public bool Serialize(Stream stream)
        {
            stream.PacketWriteUShort((ushort) this.ControlMessage);

            switch (this.ControlMessage)
            {
                case ControlMessageEnum.ConnectSyn:
                    return this.ControlSynPacketData.Serialize(stream);
                case ControlMessageEnum.ConnectSynAck:
                case ControlMessageEnum.ConnectAck:
                    return this.ControlAckPacketData.Serialize(stream);
            }

            return false;
        }

        public bool Deserialize(Stream stream)
        {
            ushort controlMessageAsShort;
            stream.PacketReadUShort(out controlMessageAsShort);
            this.ControlMessage = (ControlMessageEnum) controlMessageAsShort;

            switch (this.ControlMessage)
            {
                case ControlMessageEnum.ConnectSyn:
                    return this.ControlSynPacketData.Deserialize(stream);
                case ControlMessageEnum.ConnectSynAck:
                case ControlMessageEnum.ConnectAck:
                    return this.ControlAckPacketData.Deserialize(stream);
            }

            return false;
        }
    }

    public struct ControlSynPacketData
    {
        public uint SequenceNumber;

        public bool Serialize(Stream stream)
        {
            stream.PacketWriteUInt(this.SequenceNumber);

            return true;
        }

        public bool Deserialize(Stream stream)
        {
            stream.PacketReadUInt(out this.SequenceNumber);

            return true;
        }
    }

    public struct ControlAckPacketData
    {
        public uint SequenceNumber;
        public uint AcknowledgeNumber;

        public bool Serialize(Stream stream)
        {
            stream.PacketWriteUInt(this.SequenceNumber);
            stream.PacketWriteUInt(this.AcknowledgeNumber);

            return true;
        }

        public bool Deserialize(Stream stream)
        {
            stream.PacketReadUInt(out this.SequenceNumber);
            stream.PacketReadUInt(out this.AcknowledgeNumber);

            return true;
        }
    }


}
