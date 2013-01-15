using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using RabbitMQ.Client;

namespace SignalR.RabbitMQ
{
    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseRabbitMq(this IDependencyResolver resolver, ConnectionFactory connectionfactory, string rabbitMqExchangeName)
        {
            if (string.IsNullOrEmpty(rabbitMqExchangeName))
            {
                throw new ArgumentNullException("rabbitMqExchangeName");
            }

            if (connectionfactory == null)
            {
                throw new ArgumentNullException("connectionfactory");
            }

            var bus = new Lazy<RabbitMqMessageBus>(() => new RabbitMqMessageBus(resolver, connectionfactory, rabbitMqExchangeName));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}
