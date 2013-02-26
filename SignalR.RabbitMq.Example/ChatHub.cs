using Microsoft.AspNet.SignalR;

namespace SignalR.RabbitMq.Example
{
    public class Chat : Hub
    {
        public void Send(string message)
        {
            Clients.All.addMessage(message, Context.ConnectionId);
        }
    }
}