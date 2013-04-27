SignalR.RabbitMq
================

About
-----
SignalR.RabbitMq is an implementation of an ScaleOutMessageBus using RabbitMq as the backing store and would be used to allow a
signalr web application to be scaled across a web farm.

Installation
------------

A compiled library is available via NuGet

To install via the nuget package console

```PS
Install-Package SignalR.RabbitMq -Pre
```

To install via the nuget user interface in Visual Studio the package to search for is "SignalR.RabbitMq"

Usage
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


var exchangeName = "SignalR.RabbitMQ-Example";

var configuration = new RabbitMqScaleoutConfiguration(factory, exchangeName);
GlobalHost.DependencyResolver.UseRabbitMq(configuration);

```


The SignalR.RabbitMq message bus expects to be handed a RabbitMqScaleoutConfiguration configured with either an instance of a configured ConnectionFactory as produced by the RabbitMq.Client or a ampq connection string e.g. "host=myServer;virtualHost=myVirtualHost;username=myusername;password=topsecret" and the name of a message exchange to be used for the signalr messages.

The message bus will then create the exchange if it does not already exist then listen on an anonymous queue for messages across the web farm. There will be one queue per server in the web farm. 

It is recommended that each application should specify its own application name.

The message exchange should only be used for signalr messages.


Send to client via message bus
------------------------------

One benefit of using the message bus is to send messages directly to connected clients from another process.

From the SignalR.RabbitMQ.Console project -

```CSHARP
var factory = new ConnectionFactory 
{ 
	UserName = "guest",
	Password = "guest"
};

var exchangeName = "SignalR.RabbitMQ-Example";

var configuration = new RabbitMqScaleoutConfiguration(factory, exchangeName);
GlobalHost.DependencyResolver.UseRabbitMq(configuration);

var hubContext = GlobalHost.ConnectionManager.GetHubContext<Chat>();

Task.Factory.StartNew(
	() =>
		{
			while (true)
			{
				hubContext.Clients.All.onConsoleMessage("Hello!");
				Thread.Sleep(1000);
			}
		}
	);

```

The onConsoleMessage method is a javascript function on the client.
The message "Hello!" is put onto the message bus and relayed by the web application to the connected clients.

Advanced
--------

Everyone likes to be in control so if you have a specific requirements on connecting to RabbitMQ or if you need to audit connections or messages you can supply your own class that extends RabbitConnectionBase.

Your class can then be wired up using the other overload -

```CSHARP
GlobalHost.DependencyResolver.UseRabbitMqAdvanced(...);
```

Please see the implementation of EasyNetQRabbitConnection for an example implementation.

FAQ
---

The library uses EasyNetQ as a sane wrapper of the RabbitMQ.Client

Contributers
------------

Thanks to -

[kevingorski](https://github.com/kevingorski)
