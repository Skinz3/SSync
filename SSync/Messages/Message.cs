using SSync.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSync.Messages
{
    public abstract class Message
    {
        private const byte BIT_RIGHT_SHIFT_LEN_PACKET_ID = 2;
        private const byte BIT_MASK = 3;

        public abstract ushort MessageId
        {
            get;
        }
        public void Unpack(BigEndianReader reader)
        {
            this.Deserialize(reader);
        }
        public void Pack(BigEndianWriter header)
        {
            var data = new BigEndianWriter();
            Serialize(data);
            var size = data.Data.Length;
            var compute = ComputeTypeLen(size);
            short val = (short)((MessageId << 2) | compute);
            header.WriteShort(val);
            switch (compute)
            {
                case 1:
                    header.WriteByte((byte)size);
                    break;
                case 2:
                    header.WriteUShort((ushort)size);
                    break;
                case 3:
                    header.WriteByte((byte)((size >> 0x10) & 0xff));
                    header.WriteUShort((ushort)(size & 0xffff));
                    break;
            }
            header.WriteBytes(data.Data);
            data.Dispose();
           
        }
        public abstract void Serialize(BigEndianWriter writer);
        public abstract void Deserialize(BigEndianReader reader);

        private static byte ComputeTypeLen(int param1)
        {
            byte result;
            if (param1 > 65535)
            {
                result = 3;
            }
            else
            {
                if (param1 > 255)
                {
                    result = 2;
                }
                else
                {
                    if (param1 > 0)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
            }
            return result;
        }
        private static uint SubComputeStaticHeader(uint id, byte typeLen)
        {
            return id << 2 | (uint)typeLen;
        }
        public override string ToString()
        {
            return base.GetType().Name;
        }
    }
}
