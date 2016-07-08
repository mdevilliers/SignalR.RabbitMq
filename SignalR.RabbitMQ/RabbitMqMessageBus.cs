using System;
using System.Collections.Concurrent;
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

	    private static readonly BlockingCollection<RabbitMqMessageWrapper> Sendingbuffer
                = new BlockingCollection<RabbitMqMessageWrapper>(new ConcurrentQueue<RabbitMqMessageWrapper>());
        private static readonly BlockingCollection<RabbitMqMessageWrapper> Receivingbuffer
                = new BlockingCollection<RabbitMqMessageWrapper>(new ConcurrentQueue<RabbitMqMessageWrapper>());

	    private int _resource;

        public RabbitMqMessageBus(  IDependencyResolver resolver, 
                                    RabbitMqScaleoutConfiguration configuration, 
                                    RabbitConnectionBase advancedConnectionInstance = null)
            : base(resolver, configuration)
        {
	        if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _rabbitConnectionBase = advancedConnectionInstance ?? new RabbitConnection(configuration);
            _rabbitConnectionBase.OnDisconnectionAction = OnConnectionLost;
            _rabbitConnectionBase.OnReconnectionAction = ConnectToRabbit;
            _rabbitConnectionBase.OnMessageReceived = wrapper => Receivingbuffer.Add(wrapper);

            ConnectToRabbit();

            Task.Factory.StartNew(()=>
            {
	            while (true)
	            {
		            foreach (var message in Receivingbuffer.GetConsumingEnumerable())
		            {
			            try
			            {
				            OnReceived(0, message.Id, message.ScaleoutMessage);
			            }
			            catch
			            {
				            OnConnectionLost();
			            }
		            }
	            }
            });
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

            Task.Factory.StartNew(() =>
            {
	            while (true)
	            {
		            foreach (var message in Sendingbuffer.GetConsumingEnumerable())
		            {
			            try
			            {
				            _rabbitConnectionBase.Send(message);
				            if (message.Tcs != null)
				            {
					            message.Tcs.TrySetResult(null);
				            }
			            }
			            catch (Exception ex)
			            {
				            OnConnectionLost();
				            if (message.Tcs != null)
				            {
					            message.Tcs.TrySetException(ex);
				            }
			            }
		            }
	            }
            });

        }
        
        protected override Task Send(IList<Message> messages)
        {
            var buffer = new RabbitMqMessageWrapper(messages)
            {
                Tcs = new TaskCompletionSource<object>()
            };
            Sendingbuffer.Add(buffer);
            return buffer.Tcs.Task;
        }
    }
}