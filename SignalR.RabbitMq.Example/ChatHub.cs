using Microsoft.AspNet.SignalR;

namespace SignalR.RabbitMq.Example
{
    public class Chat : Hub
    {
        public void Send(string message)
        {
            int i = 0;
            while (true)
            {
                Clients.All.addMessage(i++, Context.ConnectionId);
            }
            // Clients.Caller.addMessage("Message sent", "you");
        }
    }
}