using System;
using System.Runtime.Serialization;

namespace SignalR.RabbitMQ
{
    [Serializable]
    public class RabbitMessageBusException : Exception
    {
        public RabbitMessageBusException()
        {
        }

        public RabbitMessageBusException(string message) : base(message)
        {
        }

        public RabbitMessageBusException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RabbitMessageBusException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
