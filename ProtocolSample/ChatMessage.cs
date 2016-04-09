using SSync.IO;
using SSync.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolSample
{
    public class ChatMessage : Message
    {
         public const ushort Id = 2;

        public string str;
      
        public override ushort MessageId
        {
            get
            {
                return Id;
            }
        }
        public ChatMessage()
        {
        }
        public ChatMessage(string str)
        {
            this.str = str;
        }
        public override void Serialize(BigEndianWriter writer)
        {
            writer.WriteUTF(this.str);

        }
        public override void Deserialize(BigEndianReader reader)
        {
            this.str = reader.ReadUTF();
        }
    }
}
