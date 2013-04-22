using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;

namespace SignalR.RabbitMQ
{
    internal class EasyNetQRabbitConnection : RabbitConnectionBase
    {
        private readonly IAdvancedBus _bus;   
        private IQueue _queue;
        private IExchange _exchange;

        public EasyNetQRabbitConnection(RabbitMqScaleoutConfiguration configuration) 
            : base(configuration)
        {
            _bus = RabbitHutch.CreateBus(configuration.AmpqConnectionString).Advanced;
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
                channel.Publish(_exchange, string.Empty, messageToSend);
            }   
        }

        public override void StartListening()
        {
            _exchange = Exchange.DeclareFanout(Configuration.ExchangeName);

            _queue = Configuration.QueueName == null
                ? Queue.DeclareTransient()
                : Queue.DeclareTransient(Configuration.QueueName);

            _queue.BindTo(_exchange, "#");
            _bus.Subscribe<RabbitMqMessageWrapper>(_queue,
                (msg, messageReceivedInfo) =>
                    Task.Factory.StartNew(() =>  OnMessage(msg.Body)));
        }

        public override void Dispose()
        {
            _bus.Dispose();
            base.Dispose();
        }
    }
}