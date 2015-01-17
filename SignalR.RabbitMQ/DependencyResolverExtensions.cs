using System;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RabbitMQ
{
    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseRabbitMq(this IDependencyResolver resolver, RabbitMqScaleoutConfiguration configuration)
        {
            return RegisterBus(resolver, configuration);
        }

        public static IDependencyResolver UseRabbitMqAdvanced(this IDependencyResolver resolver, RabbitConnectionBase myConnection, RabbitMqScaleoutConfiguration configuration)
        {
            return RegisterBus(resolver, configuration, myConnection);
        }

        private static IDependencyResolver RegisterBus(IDependencyResolver resolver, RabbitMqScaleoutConfiguration configuration, RabbitConnectionBase advancedConnectionInstance = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            RabbitMqMessageBus bus = null;
            var initialized = false;
            var syncLock = new object();
            Func<RabbitMqMessageBus> busFactory = () => new RabbitMqMessageBus(resolver, configuration, advancedConnectionInstance);

            resolver.Register(typeof (IMessageBus), () => LazyInitializer.EnsureInitialized(ref bus, ref initialized, ref syncLock, busFactory));
            
            return resolver;
        }
    }
}
