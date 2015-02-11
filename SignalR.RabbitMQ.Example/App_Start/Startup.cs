using Microsoft.Owin;
using Owin;
using SignalR.RabbitMQ.Example.App_Start;

[assembly: OwinStartup(typeof(Startup))]
namespace SignalR.RabbitMQ.Example.App_Start
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
