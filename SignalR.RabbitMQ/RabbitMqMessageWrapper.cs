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
        public RabbitMqMessageWrapper(ulong messageIdentifier, Message[] messages)
        {
            Id = messageIdentifier;
            Messages = messages;
        }

        public ulong Id { get; set; }
        public byte[] Bytes { get; set; }

        [NonSerialized]
        [JsonIgnore]
        private Message[] _messages;

        [JsonIgnore]
        public Message[] Messages { 
            get
            {
                if (_messages == null)
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

                        _messages = messages.ToArray();
                    }
                }
                return _messages;
            }
            set
            {
                if (value != null)
                {
                    _messages = value;
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
    }
}