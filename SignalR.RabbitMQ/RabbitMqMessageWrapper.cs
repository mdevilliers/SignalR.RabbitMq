using System;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RabbitMQ
{
    [Serializable]
    public class RabbitMqMessageWrapper
    {
        public RabbitMqMessageWrapper(string key, Message[] message)
        {
            Id = GetNext();
            Key = key;
            Messages = message;
        }

        public ulong Id { get; set; }
        public string Key { get; set; }
        public Message[] Messages { get; set; }

        private static ulong GetNext()
        {
            return (ulong) DateTime.Now.Ticks;
        }
    }
}