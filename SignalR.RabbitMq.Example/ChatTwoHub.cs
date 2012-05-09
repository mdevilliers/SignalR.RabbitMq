using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;

namespace SignalR.RabbitMq.Example
{
    [HubName( "chatTwo")]
    public class ChatTwoHub : Hub
    {
        public void Send(string message)
        {
            // Call the addMessage method on all clients
            Clients.addMessage("chat2 " + message);
        }
    }
}