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
        private readonly RabbitConnection _rabbitConnection;
        private readonly RabbitMqScaleoutConfiguration _configuration;
        private int _resource = 0;

        public RabbitMqMessageBus(IDependencyResolver resolver, RabbitMqScaleoutConfiguration configuration)
            : base(resolver, configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            _configuration = configuration;
            _rabbitConnection = new RabbitConnection(_configuration, ConnectToRabbit , OnConnectionLost);
            _rabbitConnection.OnMessage( wrapper => OnReceived(0, wrapper.Id, wrapper.Messages));

            ConnectToRabbit();
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && _rabbitConnection != null)
			{
				_rabbitConnection.Dispose();
			}

			base.Dispose(disposing);
		}

        protected void OnConnectionLost()
        {
            Interlocked.Exchange(ref _resource, 0);
            OnError(0, new RabbitMessageBusException("Connection to Rabbit lost."));
        }

        protected void ConnectToRabbit()
        {
            if (1 == Interlocked.Exchange(ref _resource, 1))
            {
                return;
            }
            _rabbitConnection.StartListening();
            Open(0); 
        }
        
        protected override Task Send(IList<Message> messages)
        {
            return Task.Factory.StartNew(msgs =>
                        {
                            try
                            {
                                var messagesToSend = msgs as Message[];
                                var message = new RabbitMqMessageWrapper(messagesToSend);
                                _rabbitConnection.Send(message);
                            }
                            catch
                            {
                                OnConnectionLost();
                            }
                        },
                    messages.ToArray());
        }
    }
}