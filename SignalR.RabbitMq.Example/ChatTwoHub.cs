
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.RabbitMq.Example
{
    [HubName( "chatTwo")]
    public class ChatTwoHub : Chat
    {
        public void Send(string message)
        {
            Clients.All.addMessage("chat2 " + message);
        }
    }
}