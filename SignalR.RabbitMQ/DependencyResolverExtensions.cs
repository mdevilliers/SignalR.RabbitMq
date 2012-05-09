using System;
using RabbitMQ.Client;

namespace SignalR.RabbitMQ
{
    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseRabbitMq(this IDependencyResolver resolver, string rabbitMqExchangeName, IModel rabbitMqChannel)
        {
            var bus = new Lazy<RabbitMqMessageBus>(() => new RabbitMqMessageBus(resolver, rabbitMqExchangeName,rabbitMqChannel));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}
