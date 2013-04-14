using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RabbitMQ
{
    internal class RabbitMqMessageBus : ScaleoutMessageBus
    {
        private RabbitConnection _rabbitConnection;
        private int _resource = 0;

        public RabbitMqMessageBus(IDependencyResolver resolver, RabbitMqScaleoutConfiguration configuration)
            : base(resolver, configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ConnectToRabbit(configuration);
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && _rabbitConnection != null)
			{
				_rabbitConnection.Dispose();
			}

			base.Dispose(disposing);
		}

        private void ConnectToRabbit(RabbitMqScaleoutConfiguration configuration)
        {
            if (1 == Interlocked.Exchange(ref _resource, 1))
            {
                return;
            }

            _rabbitConnection = new RabbitConnection(configuration);
            _rabbitConnection.OnMessage( 
                wrapper =>
                    {
                        OnReceived(0, wrapper.Id, wrapper.Messages);
                    }
            );
            _rabbitConnection.StartListening();
            Open(0); 
        }
        
        protected override Task Send(IList<Message> messages)
        {
            return Task.Factory.StartNew(msgs =>
            {
                var messagesToSend = msgs as Message[];
                if (messagesToSend != null)
                {
                    messagesToSend.GroupBy(m => m.Source).ToList().ForEach(group =>
                                        {
                                            var message =
                                                new RabbitMqMessageWrapper(group.Key, group.ToArray());
                                            _rabbitConnection.Send(message);
                                        });
                }
            },
            messages.ToArray()).ContinueWith(
                  t =>
                  {
                      throw new RabbitMessageBusException("SignalR.RabbitMQ error sending message. Please check your RabbitMQ connection.");
                  },
                  CancellationToken.None,
                  TaskContinuationOptions.OnlyOnFaulted,
                  TaskScheduler.Default
            ); ;
        }
    }
}