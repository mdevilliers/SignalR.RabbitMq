
using Microsoft.AspNet.SignalR;

namespace SignalR.RabbitMq.Example
{
    public class Chat : Hub
    {
        private static int _id = 0;
        public void Send(string message)
        {
            for (int i = 0; i < 20; i++)
            {
                Clients.All.addMessage(string.Format("{0} - {1}", message, _id++), Context.ConnectionId);   
            }
        }
    }
}