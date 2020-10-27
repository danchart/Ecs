using Common.Core;
using Game.Networking.Core;
using Game.Simulation.Core;
using System.IO;

namespace Game.Networking.PacketData
{
    public struct PlayerData
    {
        public const int FieldCount = 1;

        // 0
        public PlayerId Id;

        public int Serialize(BitField hasFields, Stream stream, bool measureOnly)
        {
            int size = 0;

            if (hasFields.IsSet(0))
            {
                size += stream.PacketWriteByte(0, measureOnly);
                size += stream.PacketWriteInt(this.Id, measureOnly);
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
                        {
                            int idAsInt;
                            stream.PacketReadInt(out idAsInt);

                            this.Id = new PlayerId(idAsInt);
                        }
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }
    }

    public static class PlayerDataExtensions
    {
        public static void ToPacket(
            this in PlayerComponent component, 
            ref PlayerData packet)
        {
            packet.Id = component.Id;
        }

        public static void FromPacket(
            this in PlayerData packet,
            in BitField hasFields,
            ref PlayerComponent component)
        {
            component.Id = hasFields.Bit0 ? packet.Id : component.Id;
        }

        public static void Merge(
            this PlayerData data,
            in PlayerData newData,
            ref BitField hasFields)
        {
            // 0/Id
            if (data.Id != newData.Id)
            {
                data.Id = newData.Id;
                hasFields.Set(0);
            }
        }
    }
}
