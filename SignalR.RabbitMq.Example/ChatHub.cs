using Microsoft.AspNet.SignalR;

namespace SignalR.RabbitMq.Example
{
    public class Chat : Hub
    {
        public void Send(string message)
        {

            while (true)
            {
                Clients.All.addMessage(message, Context.ConnectionId);
            }
            // Clients.Caller.addMessage("Message sent", "you");
        }
    }
}