using SSync.IO;
using SSync.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSync
{
    /// <summary>
    /// SSync Library written by Skinz 2016 All Rights Reserved
    /// This class is the core of SSync, it handle message & theirs handlers registration
    /// </summary>
    public class SSyncCore
    {
        /// <summary>
        /// Says if the protocol had been loaded or not
        /// </summary>
        public static bool Initialized = false;
        /// <summary>
        /// Message Handler Default Parameters
        /// </summary>
        private static readonly Type[] HandlerMethodParameterTypes = new Type[] { typeof(Message), typeof(SSyncClient) };
        /// <summary>
        /// Represents all the handlers methods linked to their messageIds.
        /// </summary>
        private static readonly Dictionary<uint, Delegate> Handlers = new Dictionary<uint, Delegate>();
        /// <summary>
        /// Represents all the messagesIds linked to their class type.
        /// </summary>
        private static readonly Dictionary<ushort, Type> Messages = new Dictionary<ushort, Type>();
        /// <summary>
        /// Represents all the messagesIds linked to their constructor.
        /// </summary>
        private static readonly Dictionary<ushort, Func<Message>> Constructors = new Dictionary<ushort, Func<Message>>();

        public delegate void ProtocolLoadedDelegate(int messagesCount, int handlersCount);
        /// <summary>
        /// Called when the protocol is loaded (cf: Initialize() Method)
        /// </summary>
        public static event ProtocolLoadedDelegate OnProtocolLoaded = null;

        public delegate void NotProtocolDataReceivedDelegate(SSyncClient client);
        /// <summary>
        /// Called when the SSyncClient or SSyncServer receive datas that didnt correspond to any message (Unable to unpack data)
        /// </summary>
        public static event NotProtocolDataReceivedDelegate OnUnknowDataReceived = null;
        /// Called when the handler method of a message trow an exception.
        /// </summary>
        public static event Action<Message, Exception> OnHandleFailed = null;
        /// <summary>
        /// Called when a SSyncClient or a SSyncServer receive a message with is not handled in the handler assembly.
        /// </summary>
        public static event Action<Message> OnMessageWithoutHandlerReceived = null;
        /// <summary>
        /// Initialize the SSync Library (Messages and Protocol) and call OnProtocolLoaded();
        /// </summary>
        /// <param name="messagesAssembly"></param>
        /// <param name="handlersAssembly"></param>
        public static void Initialize(Assembly messagesAssembly, Assembly handlersAssembly)
        {

            foreach (var type in messagesAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message))))
            {
                FieldInfo field = type.GetField("Id");
                if (field != null)
                {
                    ushort num = (ushort)field.GetValue(type);
                    if (Messages.ContainsKey(num))
                    {
                        throw new AmbiguousMatchException(string.Format("MessageReceiver() => {0} item is already in the dictionary, old type is : {1}, new type is  {2}",
                            num, Messages[num], type));
                    }
                    Messages.Add(num, type);
                    ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        throw new Exception(string.Format("'{0}' doesn't implemented a parameterless constructor", type));
                    }
                    Constructors.Add(num, constructor.CreateDelegate<Func<Message>>());
                }
            }

            foreach (var item in handlersAssembly.GetTypes())
            {
                foreach (var subItem in item.GetMethods())
                {
                    var attribute = subItem.GetCustomAttribute(typeof(MessageHandlerAttribute));
                    if (attribute != null)
                    {
                        ParameterInfo[] parameters = subItem.GetParameters();
                        Type methodParameters = subItem.GetParameters()[0].ParameterType;
                        if (methodParameters.BaseType != null)
                        {
                            try
                            {
                                Delegate target = subItem.CreateDelegate(HandlerMethodParameterTypes);
                                FieldInfo field = methodParameters.GetField("Id");
                                Handlers.Add((ushort)field.GetValue(null), target);
                            }
                            catch
                            {
                                throw new Exception("Cannot register " + subItem.Name + " has message handler...");
                            }

                        }
                    }

                }
            }
            Initialized = true;
            if (OnProtocolLoaded != null)
                OnProtocolLoaded(Messages.Count, Handlers.Count);
        }
        /// <summary>
        /// Unpack message
        /// </summary>
        /// <param name="id">Id of the message</param>
        /// <param name="reader">Reader with the message datas</param>
        /// <returns></returns>
        private static Message ConstructMessage(ushort id, BigEndianReader reader)
        {
            if (!Messages.ContainsKey(id))
            {
                return null;
            }
            Message message = Constructors[id]();
            if (message == null)
            {
                return null;
            }
            message.Unpack(reader);
            return message;
        }
        /// <summary>
        /// Build a messagePart and call the ConstructMessage(); method.
        /// </summary>
        /// <param name="buffer">data received</param>
        /// <returns>Message of your protocol, builted</returns>
        public static Message BuildMessage(byte[] buffer)
        {
            var reader = new BigEndianReader(buffer);
            var messagePart = new MessagePart(false);

            if (messagePart.Build(reader))
            {
                Message message;
                try
                {
                    message = ConstructMessage((ushort)messagePart.MessageId.Value, reader);
                    return message;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while building Message :" + ex.Message);
                    return null;
                }
                finally
                {
                    reader.Dispose();
                }
            }
            else
                return null;

        }
        /// <summary>
        /// Try to handle a message by finding an handler linked to this messae.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        /// <returns>True if the message is handled, False if its not</returns>
        public static bool HandleMessage(Message message, SSyncClient client)
        {
            if (!Initialized)
            {
                throw new LibraryNotLoadedException("SSync Library is not initialized, call the method SSyncCore.Initialize() before launch sockets");
            }

            if (message == null)
            {
                if (SSyncCore.OnUnknowDataReceived != null)
                    SSyncCore.OnUnknowDataReceived(client);
                return false;
            }

            var handler = Handlers.FirstOrDefault(x => x.Key == message.MessageId);

            if (handler.Value != null)
            {
                {
                    try
                    {

                        handler.Value.DynamicInvoke(null, message, client);
                        return true;

                    }
                    catch (Exception ex)
                    {
                        if (OnHandleFailed != null)
                            OnHandleFailed(message, ex);
                        return false;
                    }
                }
            }
            else
            {
                if (OnMessageWithoutHandlerReceived != null)
                    OnMessageWithoutHandlerReceived(message);
                return false;
            }
        }
    }
    /// <summary>
    /// Exception thrown when the SSync Library is not loaded
    /// </summary>
    public class LibraryNotLoadedException : Exception
    {
        public LibraryNotLoadedException() { }
        public LibraryNotLoadedException(string message) : base(message) { }
        public LibraryNotLoadedException(string message, Exception inner) : base(message, inner) { }
        protected LibraryNotLoadedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
