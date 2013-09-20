
using Microsoft.AspNet.SignalR;

namespace SignalR.RabbitMq.Console
{
    public class Chat : Hub
    {
        private static int _id = 0;
        public void Send(string message)
        {
            Clients.All.addMessage(string.Format("{0} - {1}", message, _id++), Context.ConnectionId);
            Clients.Others.addMessage(string.Format("Some one said - {0} - {1}", message, _id++), Context.ConnectionId);
        }
    }
}
