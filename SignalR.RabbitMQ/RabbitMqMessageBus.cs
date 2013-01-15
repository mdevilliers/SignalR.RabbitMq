using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using RabbitMQ.Client;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.RabbitMQ
{
    internal class RabbitMqMessageBus : ScaleoutMessageBus
    {
        private RabbitConnection _rabbitConnection;
        private Task _rabbitConnectiontask;
        private int _resource = 0;

        public RabbitMqMessageBus(IDependencyResolver resolver, ConnectionFactory connectionfactory, string rabbitMqExchangeName)
            : base(resolver)
        {
            ConnectToRabbit( connectionfactory, rabbitMqExchangeName);
        }

        public void Dispose()
        {
            if(_rabbitConnection != null)
            {
                _rabbitConnection.Dispose();
            }
            base.Dispose();
        }

        private void ConnectToRabbit(ConnectionFactory connectionfactory, string rabbitMqExchangeName)
        {
            if (1 == Interlocked.Exchange(ref _resource, 1))
            {
                return;
            }

            _rabbitConnection = new RabbitConnection(connectionfactory, rabbitMqExchangeName);
            _rabbitConnection.OnMessage( wrapper => OnReceived(wrapper.Key, wrapper.Id, wrapper.Messages));

            _rabbitConnectiontask = _rabbitConnection.StartListening();
            _rabbitConnectiontask.ContinueWith(
                t =>
                    {
                        Interlocked.Exchange(ref _resource, 0);
                        ConnectToRabbit(connectionfactory, rabbitMqExchangeName);
                    }
                );
            _rabbitConnectiontask.ContinueWith(
                  t =>
                  {
                      Interlocked.Exchange(ref _resource, 0);
                      ConnectToRabbit(connectionfactory, rabbitMqExchangeName);
                  },
                  CancellationToken.None,
                  TaskContinuationOptions.OnlyOnFaulted,
                  TaskScheduler.Default
            );

        }

        protected override Task Send(Message[] messages)
        {
            return Task.Factory.StartNew(msgs =>
            {
                var messagesToSend = msgs as Message[];
                messagesToSend.GroupBy(m => m.Source).ToList().ForEach(group =>
                {
                    var message = new RabbitMqMessageWrapper(group.Key, group.ToArray());
                    _rabbitConnection.Send(message);

                 });
            },
            messages);
        }
    }
}