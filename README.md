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

General Usage
-------------

The example web project shows how you could configure the message bus in the global.asax.cs file.

```CSHARP
var factory = new ConnectionFactory 
		{ 
			UserName = "guest",
			Password = "guest"
		};

var exchangeName = "SignalRExchange";
GlobalHost.DependencyResolver.UseRabbitMq(factory, exchangeName);
```

The SignalR.RabbitMq message bus expects to be handed an instance of a configured ConnectionFactory as produced by the RabbitMq.Client and the name of a message exchange to be used for the signalr messages.

The message bus will then create the exchange if it does not already exist then listen on an anonymous queue for messages across the web farm. There will be one queue per server in the web farm.

The message exchange should only be used for signalr messages.


Listen in
---------

So you have scaled out your message bus and scaled out SignalR something awesome but what next?

It might be interesting to have other applications see what messages are going back and forward. 

From the SignalR.RabbitMQ.Console project -

```CSHARP
var connectionfactory = new ConnectionFactory
		{
			UserName = "guest",
			Password = "guest"
		};

var rabbitMqExchangeName = "SignalRExchange";

_rabbitConnection = new RabbitConnection(connectionfactory, rabbitMqExchangeName);

var eavesdropper = new RabbitConnectionEavesdropper(_rabbitConnection);

//these are message sent to and from the Chat Hub in the web project.
eavesdropper.ListenInOnClientMessages("userJoined", invocation => { System.Console.WriteLine("User joined with connectionid : {0}", invocation.Args[0]); });
eavesdropper.ListenInOnClientMessages("addMessage", invocation => { System.Console.WriteLine("Message Sent : {0}", invocation.Args[0]); });
eavesdropper.ListenInOnClientMessages("onDisconnected", invocation => { System.Console.WriteLine("User disconnected with connectionid : {0}", invocation.Args[0]); });

_rabbitConnection.StartListening();

```

FAQ
---

The library uses the RabbitMq.Client for better or worse.