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

        public RabbitMqMessageBus(IDependencyResolver resolver, string ampqConnectionString, string applicationName)
            : base(resolver)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("applicationName");
            }

            if (string.IsNullOrEmpty(ampqConnectionString))
            {
                throw new ArgumentNullException("ampqConnectionString");
            }

            ConnectToRabbit(ampqConnectionString, applicationName);
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && _rabbitConnection != null)
			{
				_rabbitConnection.Dispose();
			}

			base.Dispose(disposing);
		}

        private void ConnectToRabbit(string ampqConnectionString, string applicationName)
        {
            if (1 == Interlocked.Exchange(ref _resource, 1))
            {
                return;
            }

            _rabbitConnection = new RabbitConnection(ampqConnectionString, applicationName);
            _rabbitConnection.OnMessage( 
                wrapper =>
                    {
                        OnReceived(wrapper.Key, wrapper.Id, wrapper.Messages);
                    }
            );
            _rabbitConnection.StartListening();
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