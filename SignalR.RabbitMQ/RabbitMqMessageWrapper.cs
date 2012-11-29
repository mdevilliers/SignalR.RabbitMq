using System;
using System.Text;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace SignalR.RabbitMQ
{
    [Serializable]
    public class RabbitMqMessageWrapper
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

        public RabbitMqMessageWrapper(string key, Message[] message)
        {
            Id = GetNext();
            Key = key;
            Messages = message;
        }

        public ulong Id { get; set; }
        public string Key { get; set; }
        public Message[] Messages { get; set; }

        public byte[] GetBytes()
        {
            var s = JsonConvert.SerializeObject(this);
            return _encoding.GetBytes(s);
        }

        public static RabbitMqMessageWrapper Deserialize(byte[] data)
        {
            var s = _encoding.GetString(data);
            return JsonConvert.DeserializeObject<RabbitMqMessageWrapper>(s);
        }

        private static ulong GetNext()
        {
            return (ulong) DateTime.Now.Ticks;
        }
    }
}