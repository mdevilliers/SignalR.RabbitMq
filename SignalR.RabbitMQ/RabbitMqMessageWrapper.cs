using System;
using System.Runtime.Serialization;

namespace SignalR.RabbitMQ
{
    [Serializable]
    [DataContract]
    public class RabbitMqMessageWrapper
    {
        public RabbitMqMessageWrapper()
        {
        }

        public RabbitMqMessageWrapper(string connectionId, string eventKey, object value)
        {
            ConnectionIdentifier = connectionId;
            EventKey = eventKey;
            Value = value.ToString();
        }

        [DataMember]
        public string EventKey { get; private set; }
        [DataMember]
        public string ConnectionIdentifier { get; private set; }
        [DataMember]
        public object Value { get; private set; }
    }
}