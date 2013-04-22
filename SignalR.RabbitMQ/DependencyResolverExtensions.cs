using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using RabbitMQ.Client;

namespace SignalR.RabbitMQ
{
    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseRabbitMq(this IDependencyResolver resolver, RabbitMqScaleoutConfiguration configuration)
        {
            if(configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var bus = new Lazy<RabbitMqMessageBus>(() => new RabbitMqMessageBus(resolver, configuration));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }

        public static IDependencyResolver UseRabbitMqAdvanced(this IDependencyResolver resolver, RabbitConnectionBase myConnection, RabbitMqScaleoutConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var bus = new Lazy<RabbitMqMessageBus>(() => new RabbitMqMessageBus(resolver, configuration, myConnection));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}
