using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;

namespace SignalR.RabbitMQ
{
    public class RabbitConnection : IDisposable
    {
        private Action<RabbitMqMessageWrapper> _handler;
        private readonly IAdvancedBus _bus;
        private readonly RabbitMqScaleoutConfiguration _configuration;
        private IQueue _queue;
        private IExchange _exchange;

        public RabbitConnection(RabbitMqScaleoutConfiguration configuration, Action onConnectionAction, Action onDisconnectionAction)
        {
            _configuration = configuration;
            _bus = RabbitHutch.CreateBus(configuration.AmpqConnectionString).Advanced;

            _bus.Connected += onConnectionAction;
            _bus.Disconnected += onDisconnectionAction; 
        }

        public void OnMessage(Action<RabbitMqMessageWrapper> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _handler= handler;
        }

        public void Send(RabbitMqMessageWrapper message)
        {
            using (var channel = _bus.OpenPublishChannel())
            {
                var messageToSend = new Message<RabbitMqMessageWrapper>(message);
                channel.Publish<RabbitMqMessageWrapper>(_exchange, string.Empty, messageToSend);
            }   
        }

        public void StartListening()
        {
            _exchange = Exchange.DeclareFanout(_configuration.ExchangeName);

            _queue = _configuration.QueueName == null
                ? Queue.DeclareTransient()
                : Queue.DeclareTransient(_configuration.QueueName);

            _queue.BindTo(_exchange, "#");
            _bus.Subscribe<RabbitMqMessageWrapper>(_queue,
                (msg, messageReceivedInfo) =>
                    Task.Factory.StartNew(() => _handler.Invoke(msg.Body)));
        }

        public void Dispose()
        {
            _bus.Dispose();
        }
    }
}