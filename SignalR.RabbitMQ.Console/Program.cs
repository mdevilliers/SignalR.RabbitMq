
using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace SignalR.RabbitMQ.Console
{
    class Program
    {
        public static RabbitConnection _rabbitConnection;

        static void Main(string[] args)
        {
            var connectionfactory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest"
            };

            var rabbitMqExchangeName = "SignalRExchange";

            _rabbitConnection = new RabbitConnection(connectionfactory, rabbitMqExchangeName);
          
            var eavesdropper = new RabbitConnectionEavesdropper(_rabbitConnection);
            
            //we are going to listen in to messages sent to and from the Chat Hub in the web project.
            eavesdropper.ListenInOnClientMessages("userJoined", (origin, invocation) => { System.Console.WriteLine("User joined with connectionid : {0}", invocation.Args[0]); });
            eavesdropper.ListenInOnClientMessages("onDisconnected", (origin, invocation) => { System.Console.WriteLine("User disconnected with connectionid : {0}", invocation.Args[0]); });
            
            eavesdropper.ListenInOnClientMessages("addMessage", (origin, invocation) =>
                                                                    {
                                                                        var receivedMessage = invocation.Args[0];
                                                                        System.Console.WriteLine("Message sent from the web application : {0}", receivedMessage);

                                                                        //create an identifier to use for this message
                                                                        var uniqueIdentifier = Guid.NewGuid().ToString();

                                                                        //create an invocation object to send a private message to the client that sent the original message
                                                                        var messageContents = new ClientHubInvocation
                                                                                                  {
                                                                                                      Hub = "Chat", 
                                                                                                      Method = "onConsoleMessage",
                                                                                                      Args = new object[] { string.Format("Hello from the console....you said {0}", receivedMessage) }
                                                                                                  };

                                                                        //wrap as a signalr message
                                                                        //sending the message to the key "Chat" would send it to all users
                                                                        //however here we are specifying a specific user using the notation {Hub}.{Origin or ConnectionIdentifier}
                                                                        var message = new Message(uniqueIdentifier,
                                                                                                  string.Format("Chat.{0}", origin),
                                                                                                  JsonConvert.SerializeObject(messageContents));
                                                                       
                                                                        //wrap as a rabbitmq message and send
                                                                        _rabbitConnection.Send(
                                                                            new RabbitMqMessageWrapper(
                                                                                uniqueIdentifier, new Message[]
                                                                                                               {
                                                                                                                  message
                                                                                                               }));

                                                                    });
            
            _rabbitConnection.StartListening();

            System.Console.ReadLine();
        }
    }
}
