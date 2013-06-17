using System;
using System.Collections;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Queue = EasyNetQ.Topology.Queue;

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
            using (var channel = _bus.OpenPublishChannel())
            {
                var messageToSend = new Message<RabbitMqMessageWrapper>(message);
                messageToSend.Properties.Headers.Add("forward_exchange", Configuration.ExchangeName);

                channel.Publish(_stampExchange, string.Empty, messageToSend);
            }   
        }

        public override void StartListening()
        {
            _receiveexchange = Exchange.DeclareFanout(Configuration.ExchangeName);
            _stampExchange = new StampExchange(Configuration.StampExchangeName);
           
            _queue = Configuration.QueueName == null
                ? Queue.DeclareTransient()
                : Queue.DeclareTransient(Configuration.QueueName);
            
            _queue.BindTo(_receiveexchange, "#");
            _bus.Subscribe<RabbitMqMessageWrapper>(_queue,
                (msg, messageReceivedInfo) =>
                    {
                        var message = msg.Body;
                        message.Id = (ulong)Convert.ToInt64(msg.Properties.Headers["stamp"]);
                        return Task.Factory.StartNew(() => OnMessage(message));
                    });
        }

        public override void Dispose()
        {
            _bus.Dispose();
            base.Dispose();
        }
    }

    public class StampExchange : Exchange
    {
        public StampExchange(string name) : this(name, "x-stamp")
        {
            
        }
        protected StampExchange(string name, string exchangeType) : base(name, exchangeType)
        {
        }

        protected StampExchange(string name, string exchangeType, bool autoDelete, IDictionary arguments) : base(name, exchangeType, autoDelete, arguments)
        {
        }
    }

}