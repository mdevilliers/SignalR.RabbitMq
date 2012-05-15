SignalR.RabbitMq
================

About
-----
SignalR.RabbitMq is an implementation of an IMessageBus using RabbitMq as the backing store and would be used to allow a
signalr web application to be scaled across a web farm.

Installation
------------

A compiled library is available via NuGet

To install via the nuget package console

```PS
Install-Package SignalR.RabbitMq
```

To install via the nuget user interface in Visual Studio the package to search for is "SignalR.RabbitMq"


Useage
------

The example project shows how you could configure the message bus in the global.asax.cs file.

```CSHARP
var exchange = "SignalRExchange";
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.ExchangeDeclare(exchange, "topic", true);
GlobalHost.DependencyResolver.UseRabbitMq(exchange, channel);
```

The SignalR.RabbitMq messagebus expects to be handed an instance of IModel as produced by the RabbitMq.Client and the name of a message exchange to be used for the signalr messages.

Any configuration or authentication for the message exchange is expected to be done before configuring the message bus.

The message exchange would usually be either be of type "topic" or "fanout". 

The message bus will then listen on an anonymous queue for messages across the web farm. There will be one queue per server in the web farm.

The message exchange should only be used for signalr messages.

The library uses the RabbitMq.Client for better or worse.