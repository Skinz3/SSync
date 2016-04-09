using ProtocolSample;
using SSync;
using SSync.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientSample
{
    class TestHandler
    {
        [MessageHandler]
        public static void HandleChatMessage(ChatMessage message, SSyncClient client)
        {
            View.Self.OnChatMessageReceived(message.str);

        }
        [MessageHandler]
        public static void HandleMessageSample(MessageSample message, SSyncClient client)
        {
            string accountName = message.userName;
        }
    }
}
