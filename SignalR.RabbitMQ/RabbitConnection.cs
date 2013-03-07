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
        private readonly IQueue _queue;
        private readonly IExchange _exchange;

        public RabbitConnection(string ampqConnectionString, string exchangeName, string queueName)
        {           
            _bus = RabbitHutch.CreateBus(ampqConnectionString).Advanced;

			_exchange = Exchange.DeclareFanout(exchangeName);

			_queue = queueName == null
				? Queue.DeclareTransient()
				: Queue.DeclareTransient(queueName);
            _queue.BindTo(_exchange, "#");
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
                    channel.Publish<RabbitMqMessageWrapper>(_exchange, string.Empty, messageToSend);
                }
            }
            catch (EasyNetQException e)
            {
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