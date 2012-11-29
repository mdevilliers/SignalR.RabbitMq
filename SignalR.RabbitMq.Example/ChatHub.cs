using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.RabbitMq.Example
{
    public class Chat : Hub
    {
        public void Send(string message)
        {
            Clients.All.addMessage(message, Context.ConnectionId);
        }

        public void NotifyJoined()
        {
            Groups.Add(Context.ConnectionId, "ChatClients");
            Clients.All.userJoined( Context.ConnectionId);
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            Clients.All.onDisconnected(Context.ConnectionId);

            return base.OnDisconnected();
        }

    }
}