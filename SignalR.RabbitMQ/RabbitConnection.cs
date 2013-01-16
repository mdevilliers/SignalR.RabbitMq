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
        private readonly string _applicationName;
        private readonly IQueue _queue;
        private readonly IExchange _exchange;

        public RabbitConnection(string ampqConnectionString, string applicationName)
        {           
            _bus = RabbitHutch.CreateBus(ampqConnectionString).Advanced;

            _queue = Queue.DeclareTransient();
            _exchange = Exchange.DeclareTopic(string.Format("{0}-{1}", "RabbitMQ.SignalR",applicationName));
            _queue.BindTo(_exchange, "#");

            _applicationName = applicationName;
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
            try
            {
                using (var channel = _bus.OpenPublishChannel())
                {
                    var messageToSend = new Message<RabbitMqMessageWrapper>(message);
                    channel.Publish<RabbitMqMessageWrapper>(_exchange, string.Empty , messageToSend);
                }
            }
            catch (EasyNetQException e)
            {
                // the server is not connected
                throw new RabbitMessageBusException("RabbitMQ channel is not open.", e);
            }    
        }

        public void StartListening()
        {
            _bus.Subscribe<RabbitMqMessageWrapper>(_queue, (msg, messageReceivedInfo) => Task.Factory.StartNew(() => _handler.Invoke(msg.Body)));
        }

        public void Dispose()
        {
            _bus.Dispose();
        }
    }
}