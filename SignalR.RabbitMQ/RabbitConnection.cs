using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SignalR.RabbitMQ
{
    public class RabbitConnection : IDisposable
    {
        private readonly ConnectionFactory _rabbitMqConnectionfactory;
        private readonly string _rabbitMqExchangeName;
        private IModel _channel;
        private IList<Action<RabbitMqMessageWrapper>> _handlers;
        private CancellationTokenSource _cancellationTokenSource;
        
        public RabbitConnection(ConnectionFactory connectionfactory, string rabbitMqExchangeName)
        {
            _rabbitMqConnectionfactory = connectionfactory;
            _rabbitMqExchangeName = rabbitMqExchangeName;
            _handlers = new List<Action<RabbitMqMessageWrapper>>();
        }

        public void OnMessage(Action<RabbitMqMessageWrapper> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _handlers.Add(handler);
        }

        public Task Send(RabbitMqMessageWrapper message)
        {
            try
            {
                if (_channel.IsOpen)
                {
                   return Task.Factory.StartNew(() => _channel.BasicPublish(_rabbitMqExchangeName,
                                                                            message.Key,
                                                                            null,
                                                                            message.GetBytes()));
                }

                throw new Exception("RabbitMQ channel is not open.");
            }
            catch (Exception exception)
            {
                TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
                completionSource.SetException(exception);
                return completionSource.Task;
            }
        }

        public Task StartListening()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            var task = Task.Factory.StartNew(() =>
                        {
                            var connection = _rabbitMqConnectionfactory.CreateConnection();
                            _channel = connection.CreateModel();
                            _channel.ExchangeDeclare(_rabbitMqExchangeName, "topic", true);

                            var queue = _channel.QueueDeclare("", false, false, true, null);
                            _channel.QueueBind(queue.QueueName, _rabbitMqExchangeName, "#");

                            var consumer = new QueueingBasicConsumer(_channel);
                            _channel.BasicConsume(queue.QueueName, false, consumer);

                            while (_channel.IsOpen && !token.IsCancellationRequested)
                            {
                                try
                                {
                                    var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                                    _channel.BasicAck(ea.DeliveryTag, false);

                                    Task.Factory.StartNew((handlers) =>
                                                              {
                                                                  var message =
                                                                      RabbitMqMessageWrapper.Deserialize(ea.Body);

                                                                  var handlersToInform =
                                                                      (IList<Action<RabbitMqMessageWrapper>>) handlers;

                                                                  foreach (var handler in handlersToInform)
                                                                  {
                                                                      handler.Invoke(message);
                                                                  }
                                                              }, _handlers);

                                }catch(EndOfStreamException eose)
                                {
                                    //ignore
                                }
                            }
                        },token);

            return task;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}