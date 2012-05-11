using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SignalR.RabbitMQ
{
    public class RabbitMqMessageBus : IMessageBus, IIdGenerator<long>
    {
        private readonly InProcessMessageBus<long> _bus;
        private readonly IModel _rabbitmqchannel;
        private readonly string _rabbitmqExchangeName;
        private int _resource = 0;
        private int _count;

        public RabbitMqMessageBus(IDependencyResolver resolver, string rabbitMqExchangeName, IModel rabbitMqChannel)
        {
            _bus = new InProcessMessageBus<long>(resolver, this);
            _rabbitmqchannel = rabbitMqChannel;
            _rabbitmqExchangeName = rabbitMqExchangeName;

            EnsureConnection();
        }

        public Task<MessageResult> GetMessages(IEnumerable<string> eventKeys, string id, CancellationToken timeoutToken)
        {
            return _bus.GetMessages(eventKeys, id, timeoutToken);
        }

        public Task Send(string connectionId, string eventKey, object value)
        {
            var message = new RabbitMqMessageWrapper(connectionId, eventKey, value);
            return Task.Factory.StartNew(SendMessage, message);
        }

        public long ConvertFromString(string value)
        {
            return Int64.Parse(value, CultureInfo.InvariantCulture);
        }

        public string ConvertToString(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public long GetNext()
        {
            return _count++;
        }

        private void SendMessage(object state)
        {
            var message = (RabbitMqMessageWrapper) state;
            byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _rabbitmqchannel.BasicPublish(_rabbitmqExchangeName, message.EventKey, null, payload);
        }

        private void EnsureConnection()
        {
            var tcs = new TaskCompletionSource<Object>();

            if (1 == Interlocked.Exchange(ref _resource, 1))
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {

                        var queue = _rabbitmqchannel.QueueDeclare("", false, false, true, null);
                        _rabbitmqchannel.QueueBind(queue.QueueName, _rabbitmqExchangeName, "#");

                        var consumer = new QueueingBasicConsumer(_rabbitmqchannel);
                        _rabbitmqchannel.BasicConsume(queue.QueueName, false, consumer);

                        while (true)
                        {
                            var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();

                            _rabbitmqchannel.BasicAck(ea.DeliveryTag, false);

                            string json = Encoding.UTF8.GetString(ea.Body);
                                                  
                            var message = JsonConvert.DeserializeObject<RabbitMqMessageWrapper>(json);
                            _bus.Send(message.ConnectionIdentifier, message.EventKey, message.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
        }
    }
}