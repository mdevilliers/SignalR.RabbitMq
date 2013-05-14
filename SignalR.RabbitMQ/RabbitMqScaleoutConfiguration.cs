using System;
using EasyNetQ;
using Microsoft.AspNet.SignalR.Messaging;
using RabbitMQ.Client;

namespace SignalR.RabbitMQ
{
    public class RabbitMqScaleoutConfiguration : ScaleoutConfiguration
    {
        public RabbitMqScaleoutConfiguration(string ampqConnectionString, string exchangeName, string queueName = null)
        {
            if (string.IsNullOrEmpty(ampqConnectionString))
            {
                throw new ArgumentNullException("ampqConnectionString");
            }

            if (string.IsNullOrEmpty(exchangeName))
            {
                throw new ArgumentNullException("exchangeName");
            }

            this.AmpqConnectionString = ampqConnectionString;
            this.ExchangeName = exchangeName;
            this.QueueName = queueName;
        }
        public RabbitMqScaleoutConfiguration(ConnectionFactory connectionfactory, string exchangeName, string queueName = null)
        {
            if (connectionfactory == null)
            {
                throw new ArgumentNullException("connectionfactory");
            }

            if (string.IsNullOrEmpty(exchangeName))
            {
                throw new ArgumentNullException("exchangeName");
            }
#if DEBUG
            //no heartbeat for debugging
            var ampqConnectionString = string.Format("host={0};virtualHost={1};username={2};password={3}", connectionfactory.HostName, connectionfactory.VirtualHost, connectionfactory.UserName, connectionfactory.Password);
#else
            var ampqConnectionString = string.Format("host={0};virtualHost={1};username={2};password={3};requestedHeartbeat=10", connectionfactory.HostName, connectionfactory.VirtualHost, connectionfactory.UserName, connectionfactory.Password);
#endif
            
            this.AmpqConnectionString = ampqConnectionString;
            this.ExchangeName = exchangeName;
            this.QueueName = queueName;
        }
        public RabbitMqScaleoutConfiguration(IBus bus, string exchangeName, string queueName = null)
        {
            if (bus == null)
            {
                throw new ArgumentNullException("bus");
            }
            
            if (string.IsNullOrEmpty(exchangeName))
            {
                throw new ArgumentNullException("exchangeName");
            }

            Bus = bus;
            this.ExchangeName = exchangeName;
            this.QueueName = queueName;
        }

        public string AmpqConnectionString { get; private set; }
        public string ExchangeName { get; private set; }
        public string QueueName { get; private set; }
        public IBus Bus { get; private set; }
    }
}