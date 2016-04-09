using SSync.IO;
using SSync.Messages;
using SSync.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SSync
{
    public class SSyncClient : Client
    {
        /// <summary>
        /// Called when a message has been sended
        /// </summary>
        public event Action<Message> OnMessageSended = null;
        /// <summary>
        /// Called When a message is received and when he is handled
        /// </summary>
        public event Action<Message> OnMessageReceived = null;
        /// <summary>
        /// <summary>
        /// Dummmy constructor
        /// </summary>
        public SSyncClient()
            : base()
        {
            base.OnDataArrival += SSyncClient_OnDataArrival;
        }

        public SSyncClient(Socket sock)
            : base(sock)
        {
            base.OnDataArrival += SSyncClient_OnDataArrival;
        }
        void SSyncClient_OnDataArrival(byte[] datas)
        {
            Message message = SSyncCore.BuildMessage(datas);

            if (SSyncCore.HandleMessage(message, this))
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(message);
            }
        }
        public void Send(Message message)
        {
            BigEndianWriter writer = new BigEndianWriter();
            message.Pack(writer);
            var packet = writer.Data;
            Send(packet);
            if (OnMessageSended != null)
                OnMessageSended(message);

        }
    }
}
