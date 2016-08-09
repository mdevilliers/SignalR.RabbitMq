using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace SignalR.RabbitMQ
{
    internal class RabbitConnection : RabbitConnectionBase
    {
        private IConnection _rabbitConnection;
        private IModel _publishModel;
        private IModel _subscribeModel;

        public RabbitConnection(RabbitMqScaleoutConfiguration configuration)
            : base(configuration) {}

        public override void Send(RabbitMqMessageWrapper message)
        {
            var properties = new BasicProperties
            {
                Headers = new Dictionary<string, object>
                {
                    { "forward_exchange", Configuration.ExchangeName }
                }
            };

            _publishModel.BasicPublish(Configuration.StampExchangeName, "", properties, message.Bytes);
        }

        public override void StartListening()
        {
            _rabbitConnection = Configuration.ConnectionFactory.CreateConnection();

            _publishModel = _rabbitConnection.CreateModel();
            _subscribeModel = _rabbitConnection.CreateModel();

            _publishModel.ExchangeDeclare(Configuration.StampExchangeName, "x-stamp", durable: true);
            _subscribeModel.ExchangeDeclare(Configuration.ExchangeName, ExchangeType.Fanout, durable: true);
            _subscribeModel.QueueDeclare(Configuration.QueueName, durable: false, exclusive: false, autoDelete: true, arguments: new Dictionary<string, object>());
            _subscribeModel.QueueBind(Configuration.QueueName, Configuration.ExchangeName, "");

            var consumer = new EventingBasicConsumer(_subscribeModel);
            consumer.Received += (sender, args) =>
            {
                try
                {
                    OnMessage(new RabbitMqMessageWrapper
                    {
                        Bytes = args.Body,
                        Id = Convert.ToUInt64(args.BasicProperties.Headers["stamp"])
                    });
                }
                finally
                {
                    _subscribeModel.BasicAck(args.DeliveryTag, multiple: false);
                }
            };

            _subscribeModel.BasicConsume(Configuration.QueueName, noAck: false, consumer: consumer);
        }

        public override void Dispose()
        {
            _publishModel.Dispose();
            _subscribeModel.Dispose();
            _rabbitConnection.Dispose();

            base.Dispose();
        }
    }
}