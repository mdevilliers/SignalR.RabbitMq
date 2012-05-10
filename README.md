SignalR.RabbitMq
================

Implementation of a IMessageBus with RabbitMq as the backing store.

To use see the global.asax in the example.
```CSHARP
var exchange = "SignalRExchange";
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.ExchangeDeclare(exchange, "topic", true);
GlobalHost.DependencyResolver.UseRabbitMq(exchange, channel);
```
Uses the RabbitMq.Client for better or worse.