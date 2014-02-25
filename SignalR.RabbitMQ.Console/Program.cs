
using Microsoft.AspNet.SignalR;
using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;
using SignalR.RabbitMq.Console;

namespace SignalR.RabbitMQ.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };

            var exchangeName = "SignalR.RabbitMQ-Example";

            var configuration = new RabbitMqScaleoutConfiguration(factory, exchangeName);
            GlobalHost.DependencyResolver.UseRabbitMq(configuration);

            var examplePacketSize = 1024*2;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Chat>();

            Task.Factory.StartNew(
                () =>
                    {
                        int i = 0;
                        while (true)
                        {
                            var message = string.Format("Message Id - {0}, Message Padded To {1} bytes." , i, examplePacketSize);
                            var noise = message.PadRight(examplePacketSize, '-');

                            hubContext.Clients.All.onConsoleMessage(noise);
                            System.Console.WriteLine(i);
                            i++;
                            Thread.Sleep(100);

                        }
                    }
                );
            System.Console.WriteLine("Press any key to exit.");
            System.Console.Read();
        }
    }
}
