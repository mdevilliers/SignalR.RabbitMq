using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;

namespace SignalR.RabbitMQ
{
    public class RabbitConnectionEavesdropper
    {
        private readonly RabbitConnection _rabbitConnection;

        public RabbitConnectionEavesdropper(RabbitConnection rabbitConnection)
        {
            if (rabbitConnection == null)
            {
                throw new ArgumentNullException("rabbitConnection");
            }
            _rabbitConnection = rabbitConnection;
        }

        public void ListenInOnClientMessages(string clientsideInvocation ,Action<string,ClientHubInvocation> handler)
        {
            if (string.IsNullOrEmpty(clientsideInvocation))
            {
                throw new ArgumentNullException("clientsideInvocation");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            _rabbitConnection.OnMessage(
              (x) =>
              {
                  var messages = x.Messages;

                  foreach (var message in messages)
                  {
                      if (!IsSignalRServerMethod(message.Key) && message.Value != null)
                      {
                          var hubMessage =
                              JsonConvert.DeserializeObject<ClientHubInvocation>(message.Value);

                          if (!string.IsNullOrEmpty(hubMessage.Method) && hubMessage.Method.Equals(clientsideInvocation))
                          {
                              handler.Invoke(message.Source, hubMessage);
                          }
                      }
                  }
              }
            );
        }

        private bool IsSignalRServerMethod(string key)
        {
            return key.Equals("__SIGNALR__SERVER__", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
