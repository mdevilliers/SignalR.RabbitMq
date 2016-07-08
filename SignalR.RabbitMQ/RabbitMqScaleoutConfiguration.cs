using System;
using Microsoft.AspNet.SignalR.Messaging;
using RabbitMQ.Client;

namespace SignalR.RabbitMQ
{
    public class RabbitMqScaleoutConfiguration : ScaleoutConfiguration
    {
        public RabbitMqScaleoutConfiguration(ConnectionFactory connectionfactory, string exchangeName, string queueName = "", string stampExchangeName = "signalr-stamp")
        {
            if (connectionfactory == null)
            {
                throw new ArgumentNullException("connectionfactory");
            }

            if (string.IsNullOrEmpty(exchangeName))
            {
                throw new ArgumentNullException("exchangeName");
            }

            ConnectionFactory = connectionfactory;
            ExchangeName = exchangeName;
            QueueName = queueName;
            StampExchangeName = stampExchangeName;
        }

        public string ExchangeName { get; private set; }
        public string StampExchangeName { get; private set; }
        public string QueueName { get; private set; }
        public ConnectionFactory ConnectionFactory { get; private set; }
    }
}