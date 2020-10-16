﻿using Game.Networking.Core;
using System.IO;

namespace Game.Networking.PacketData
{
    public struct TransformData
    {
        // 0
        public float x;
        // 1
        public float y;
        // 2
        public float rotation;

        public bool Serialize(BitField hasFields, Stream stream)
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
}
