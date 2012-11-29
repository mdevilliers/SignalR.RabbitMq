
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
            
            //these are message sent to and from the Chat Hub in the web project.
            eavesdropper.ListenInOnClientMessages("userJoined", invocation => { System.Console.WriteLine("User joined with connectionid : {0}", invocation.Args[0]); });
            eavesdropper.ListenInOnClientMessages("addMessage", invocation => { System.Console.WriteLine("Message Sent : {0}", invocation.Args[0]); });
            eavesdropper.ListenInOnClientMessages("onDisconnected", invocation => { System.Console.WriteLine("User disconnected with connectionid : {0}", invocation.Args[0]); });

            _rabbitConnection.StartListening();

            System.Console.ReadLine();
        }
    }
}
