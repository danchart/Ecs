using Networking.Core;
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
    public struct ControlPacket : IPacketSerialization
    {
        [FieldOffset(0)]
        public ControlMessageEnum ControlMessage;

        [FieldOffset(2)]
        public ControlSynPacketData ControlSynPacketData;
        [FieldOffset(2)]
        public ControlAckPacketData ControlAckPacketData;

        public int Serialize(Stream stream)
        {
            int size = stream.PacketWriteUShort((ushort) this.ControlMessage);

            switch (this.ControlMessage)
            {
                case ControlMessageEnum.ConnectSyn:
                    return size + this.ControlSynPacketData.Serialize(stream);
                case ControlMessageEnum.ConnectSynAck:
                case ControlMessageEnum.ConnectAck:
                    return size + this.ControlAckPacketData.Serialize(stream);
            }

            return -1;
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

    public struct ControlSynPacketData : IPacketSerialization
    {
        /// <summary>
        /// Client provided synchronization sequence #.
        /// </summary>
        public uint SequenceKey;

        public int Serialize(Stream stream)
        {
            int size = stream.PacketWriteUInt(this.SequenceKey);

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            stream.PacketReadUInt(out this.SequenceKey);

            return true;
        }
    }

    public struct ControlAckPacketData : IPacketSerialization
    {
        /// <summary>
        /// Client provided synchronization sequence # from original SYN request.
        /// </summary>
        public uint SequenceKey;

        /// <summary>
        /// Server provided acknowledgement # from SYN-ACK request.
        /// </summary>
        public uint AcknowledgementKey;

        public int Serialize(Stream stream)
        {
            int size = stream.PacketWriteUInt(this.SequenceKey);
            size += stream.PacketWriteUInt(this.AcknowledgementKey);

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            stream.PacketReadUInt(out this.SequenceKey);
            stream.PacketReadUInt(out this.AcknowledgementKey);

            return true;
        }
    }


}
