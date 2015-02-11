using EasyNetQ;
using EasyNetQ.Topology;
using System;
using System.Threading.Tasks;

namespace SignalR.RabbitMQ
{
    internal class EasyNetQRabbitConnection : RabbitConnectionBase
    {
        private readonly IAdvancedBus _bus;   
        private IQueue _queue;
        private IExchange _stampExchange;
        private IExchange _receiveexchange;

        public EasyNetQRabbitConnection(RabbitMqScaleoutConfiguration configuration) 
            : base(configuration)
        {
            _bus = configuration.Bus != null 
                ? configuration.Bus.Advanced 
                : RabbitHutch.CreateBus(configuration.AmpqConnectionString).Advanced;

            //wire up the reconnection handler
            _bus.Connected += OnReconnection;
            
            //wire up the disconnection handler
            _bus.Disconnected += OnDisconnection;
        }

        public override void Send(RabbitMqMessageWrapper message)
        {
            var messageToSend = new Message<byte[]>(message.Bytes);
            messageToSend.Properties.Headers.Add("forward_exchange", Configuration.ExchangeName);
            _bus.Publish(_stampExchange, string.Empty, false, false, messageToSend);
        }

        public override void StartListening()
        {
            _receiveexchange = _bus.ExchangeDeclare(Configuration.ExchangeName, ExchangeType.Fanout);
            _stampExchange = _bus.ExchangeDeclare(Configuration.StampExchangeName, "x-stamp");

            _queue = Configuration.QueueName == null
                        ? _bus.QueueDeclare()
                        : _bus.QueueDeclare(Configuration.QueueName);

            _bus.Bind( _receiveexchange, _queue, "#");
            _bus.Consume<byte[]>(_queue,
                (msg, messageReceivedInfo) =>
                    {
                        var bytes = msg.Body;
                        var message = new RabbitMqMessageWrapper
                        {
                            Bytes = bytes,
                            Id = (ulong) Convert.ToInt64(msg.Properties.Headers["stamp"])
                        };
                        return Task.Factory.StartNew(() => OnMessage(message));
                    });
        }

        public override void Dispose()
        {
            _bus.Dispose();
            base.Dispose();
        }
    }
}