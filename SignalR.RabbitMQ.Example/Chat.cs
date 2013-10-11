using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalR.RabbitMq.Example
{
    public class Chat : Hub
    {
        private static int _id = 0;
        public void Send(string message)
        {
            Clients.All.addMessage(string.Format("{0} - {1}", message, _id), Context.ConnectionId);
            Clients.Others.addMessage(string.Format("Some one said - {0} - {1}", message, _id), Context.ConnectionId);
            _id++;
        }

        private const string GroupName = "SecretGroup";
        public void SendGroup(string message)
        {
            Clients.Group(GroupName).addMessage(string.Format("{0} - {1}", message, _id), GroupName);
            _id++;
        }

        public Task JoinGroup(string groupName)
        {
            return Groups.Add(Context.ConnectionId, GroupName);
        }
    }
}
