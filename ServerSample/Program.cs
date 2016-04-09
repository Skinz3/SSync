using ProtocolSample;
using SSync;
using SSync.StartupEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerSample
{
    class Program
    {
        public static List<ChatClient> Clients = new List<ChatClient>();

        static void Main(string[] args)
        {
            SSyncCore.OnProtocolLoaded += SSyncCore_OnProtocolLoaded;

            SSyncCore.Initialize(Assembly.GetAssembly(typeof(ChatRequestMessage)), Assembly.GetExecutingAssembly());
            StartupManager.OnItemLoading += StartupManager_OnItemLoaded;
            StartupManager.OnStartupEnded += StartupManager_OnStartupEnded;
            StartupManager.Initialize(Assembly.GetExecutingAssembly());
            SSyncServer serv = new SSyncServer("127.0.0.1", 500);


            serv.OnClientConnected += serv_OnSocketAccepted;
            serv.Start();

        loop:
            string str = Console.ReadLine();
            Clients.ForEach(x => x.Send(new ChatMessage(str)));
            Console.WriteLine(str + " Sended to clients");
            goto loop;

        }

        static void StartupManager_OnStartupEnded(TimeSpan obj)
        {
            Console.WriteLine("Startup Ended " + obj.Milliseconds);
        }

        static void StartupManager_OnItemLoaded(StartupInvokeType arg1, string arg2)
        {
            Console.WriteLine("[" + arg1 + "] Loading: " + arg2);
        }

        static void serv_OnSocketAccepted(System.Net.Sockets.Socket socket)
        {
            Console.WriteLine("New Client connected");
            Clients.Add(new ChatClient(socket));
        }

        static void SSyncCore_OnProtocolLoaded(int messagesCount, int handlersCount)
        {
            Console.WriteLine(messagesCount + " messages loaded");
            Console.WriteLine(handlersCount + " handlers loaded");
        }




    }
}
