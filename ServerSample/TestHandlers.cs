using ProtocolSample;
using SSync;
using SSync.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSample
{
    class TestHandlers
    {
        [MessageHandler()]
        public static void HandleChatRequestMessage(ChatRequestMessage message,SSyncClient client)
        {
            foreach (var _client in Program.Clients)
            {
                _client.Send(new ChatMessage(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ": " + message.str));
            }

        }
    }
}
