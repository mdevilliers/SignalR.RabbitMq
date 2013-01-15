
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.RabbitMq.Example
{
    [HubName( "chatTwo")]
    public class ChatTwoHub :Hub
    {
        public void Send(string message)
        {
            Clients.All.addMessage("chat2 " + message);
        }
    }
}