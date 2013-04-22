using System;

namespace SignalR.RabbitMQ
{
    public class RabbitConnectionBase : IDisposable
    {
        public RabbitConnectionBase(RabbitMqScaleoutConfiguration configuration)
        {
            if(configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            Configuration = configuration;
        }

        internal Action OnReconnectionAction { get; set; }
        internal Action OnDisconnectionAction { get; set; }
        public RabbitMqScaleoutConfiguration Configuration { get; set; }
        internal Action<RabbitMqMessageWrapper> OnMessageRecieved { get; set; }

        public virtual void Dispose()
        {
            // do nothing?
        }

        public virtual void Send(RabbitMqMessageWrapper message)
        {
            throw new NotImplementedException("Implement the Send method in your Rabbit connection class.");
        }

        public virtual void StartListening()
        {
            throw new NotImplementedException("Implement the StartListening method in your Rabbit connection class.");
        }

        protected void OnReconnection()
        {
            if (OnReconnectionAction != null)
            {
                OnReconnectionAction.Invoke();
            }
        }

        protected void OnDisconnection()
        {
            if (OnDisconnectionAction != null)
            {
                OnDisconnectionAction.Invoke();
            }
        }

        protected void OnMessage(RabbitMqMessageWrapper message)
        {
            OnMessageRecieved.Invoke(message);
        }
    }
}