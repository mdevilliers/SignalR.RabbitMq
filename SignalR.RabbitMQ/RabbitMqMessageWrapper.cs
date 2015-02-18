using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.SignalR.Messaging;
using System.Threading.Tasks;

namespace SignalR.RabbitMQ
{
    public class RabbitMqMessageWrapper
    {
        private ScaleoutMessage _scaleoutMessage;

        public RabbitMqMessageWrapper()
        {
        }
        
        public RabbitMqMessageWrapper( IList<Message> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }
            ScaleoutMessage = new ScaleoutMessage(messages);
        }

        public ulong Id { get; set; }

        public TaskCompletionSource<object> Tcs { get; set; }

        public byte[] Bytes { get; set; }

        public ScaleoutMessage ScaleoutMessage 
        {
            get
            {
                if(_scaleoutMessage == null)
                {
                    using (var stream = new MemoryStream(Bytes))
                    {
                        var binaryReader = new BinaryReader(stream);
                        byte[] buffer = binaryReader.ReadBytes(Bytes.Length);
                        _scaleoutMessage = ScaleoutMessage.FromBytes(buffer);
                    }
                }
                return _scaleoutMessage;
            }
            set
            {
                Bytes = value.ToBytes();
                _scaleoutMessage = value;
            }
        }
    }
}