using SSync.IO;
using SSync.Messages;

namespace ProtocolSample
{
    public class MessageSample : Message
    {
        public const ushort Id = 3;

        public int accountId;

        public string userName;

        public string password;

        public string email;

        public override ushort MessageId
        {
            get
            {
                return Id;
            }
        }
        public MessageSample()
        {
        }
        public MessageSample(int accountId,string userName,string password,string email)
        {
            this.accountId = accountId;
            this.userName = userName;
            this.password = password;
            this.email = email;
        }
        public override void Serialize(BigEndianWriter writer)
        {
            writer.WriteInt(accountId);
            writer.WriteUTF(userName);
            writer.WriteUTF(password);
            writer.WriteUTF(email);

        }
        public override void Deserialize(BigEndianReader reader)
        {
            this.accountId = reader.ReadInt();
            this.userName = reader.ReadUTF();
            this.password = reader.ReadUTF();
            this.email = reader.ReadUTF();
        }
    }
}