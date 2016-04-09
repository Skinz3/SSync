using SSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerSample
{
    public class ChatClient : SSyncClient
    {
        public ChatClient(Socket socket):base(socket)
        {
            this.OnClosed += ChatClient_OnClosed;
            this.OnMessageSended += ChatClient_OnMessageSended;
            this.OnMessageReceived += ChatClient_OnMessageReceived;
        }

        void ChatClient_OnMessageReceived(SSync.Messages.Message arg1)
        {
            Console.WriteLine("Received: " + arg1.ToString());
        }

        void ChatClient_OnMessageSended(SSync.Messages.Message obj)
        {
            Console.WriteLine("sended " + obj.ToString());
        }

        void ChatClient_OnClosed()
        {
            Console.WriteLine("Client disconnected");
            Program.Clients.Remove(this);
        }
    }
}
