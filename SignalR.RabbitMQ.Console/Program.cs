
using Microsoft.AspNet.SignalR;
using RabbitMQ.Client;
using SignalR.RabbitMq.Example;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.RabbitMQ.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest"
            };

            var exchangeName = "SignalR.RabbitMQ-Example";

            var configuration = new RabbitMqScaleoutConfiguration(factory, exchangeName);
            GlobalHost.DependencyResolver.UseRabbitMq(configuration); ;

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
            System.Console.WriteLine("Press any key to exit.");
            System.Console.Read();
        }
    }
}
