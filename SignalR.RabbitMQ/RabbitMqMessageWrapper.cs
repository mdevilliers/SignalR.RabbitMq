using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;

namespace SignalR.RabbitMQ
{
    [Serializable]
    public class RabbitMqMessageWrapper
    {
        public RabbitMqMessageWrapper(string key, Message[] messages)
        {
            Id = GetNext();
            Key = key;
            Messages = messages;
        }

        public ulong Id { get; set; }
        public string Key { get; set; }
        public byte[] Bytes { get; set; }

        [JsonIgnore]
        public Message[] Messages { 
            get
            {
                using (var stream = new MemoryStream(Bytes))
                {
                    var binaryReader = new BinaryReader(stream);
                    
                    var messages = new List<Message>();
                    int count = binaryReader.ReadInt32();
                    
                    for (int i = 0; i < count; i++)
                    {
                        messages.Add(Message.ReadFrom(stream));
                    }

                    return messages.ToArray();
                }  
            }
            set
            {
                if (value != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        var binaryWriter = new BinaryWriter(ms);

                        binaryWriter.Write(value.Length);
                        for (int i = 0; i < value.Length; i++)
                        {
                            value[i].WriteTo(ms);
                        }

                        Bytes = ms.ToArray();
                    }
                }
            }
        }

        private static ulong GetNext()
        {
            return (ulong) DateTime.Now.Ticks;
        }
    }
}