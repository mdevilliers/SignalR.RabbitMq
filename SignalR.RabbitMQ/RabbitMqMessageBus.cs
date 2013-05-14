using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RabbitMQ
{
    internal class RabbitMqMessageBus : ScaleoutMessageBus
    {
        private readonly RabbitConnectionBase _rabbitConnectionBase;
        private readonly RabbitMqScaleoutConfiguration _configuration;
        private readonly UniqueMessageIdentifierGenerator _messageIdentifierGenerator;
        private int _resource = 0;

        public RabbitMqMessageBus(  IDependencyResolver resolver, 
                                    RabbitMqScaleoutConfiguration configuration, 
                                    RabbitConnectionBase advancedConnectionInstance = null)
            : base(resolver, configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            _configuration = configuration;

            if (advancedConnectionInstance != null)
            {
                advancedConnectionInstance.OnDisconnectionAction = OnConnectionLost;
                advancedConnectionInstance.OnReconnectionAction = ConnectToRabbit;
                advancedConnectionInstance.OnMessageRecieved =
                    wrapper => OnReceived(0, wrapper.Id, wrapper.ScaleoutMessage);

                _rabbitConnectionBase = advancedConnectionInstance;
            }
            else
            {
                _rabbitConnectionBase = new EasyNetQRabbitConnection(_configuration)
                                            {
                                                OnDisconnectionAction = OnConnectionLost,
                                                OnReconnectionAction = ConnectToRabbit,
                                                OnMessageRecieved = ForwardOnReceivedMessage
                                            };
            }
            _messageIdentifierGenerator = new UniqueMessageIdentifierGenerator();
            ConnectToRabbit();
        }

        private void ForwardOnReceivedMessage( RabbitMqMessageWrapper message)
        {
            _messageIdentifierGenerator.LastSeenMessageIdentifier(message.Id);
            OnReceived(0, message.Id, message.ScaleoutMessage);
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && _rabbitConnectionBase != null)
			{
				_rabbitConnectionBase.Dispose();
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
            _rabbitConnectionBase.StartListening();
            Open(0); 
        }
        
        protected override Task Send(IList<Message> messages)
        {
            return Task.Factory.StartNew(msgs =>
                        {
                            try
                            {
                                var messagesToSend = msgs as IList<Message>;
                                if (messagesToSend != null)
                                {
                                    var message = new RabbitMqMessageWrapper( _messageIdentifierGenerator.GetNextMessageIdentifier(), messagesToSend);
                                    _rabbitConnectionBase.Send(message);
                                }
                            }
                            catch
                            {
                                OnConnectionLost();
                            }
                        },
                    messages);
        }
    }
}