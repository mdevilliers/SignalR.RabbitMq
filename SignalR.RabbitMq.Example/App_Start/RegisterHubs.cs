
using System.Web;
using System.Web.Routing;

[assembly: PreApplicationStartMethod(typeof(SignalR.RabbitMq.Example.RegisterHubs), "Start")]

namespace SignalR.RabbitMq.Example
{
    public static class RegisterHubs
    {
        public static void Start()
        {
            // Register the default hubs route: ~/signalr/hubs
            RouteTable.Routes.MapHubs();            
        }
    }
}
