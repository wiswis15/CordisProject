using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;

namespace MCDBackend.Notificaitons
{
    public class UdpNotificationServer : INotificationServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UdpNotificationServer));

        private List<ClientObject> _clientList = new List<ClientObject>();

        public void AddClient(ClientObject client)
        {
            if (!_clientList.Contains(client))
            {
                _clientList.Add(client);
            }
        }

        public void RemoveClient(ClientObject client)
        {
            if (_clientList.Contains(client))
            {
                _clientList.Remove(client);
            }
        }

        public void SendNotification(string message)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] sendbuf = Encoding.ASCII.GetBytes(message);
            foreach (ClientObject client in _clientList)
            {
                try
                {
                    IPAddress clientAddress = IPAddress.Parse(client.Address);
                    IPEndPoint ep = new IPEndPoint(clientAddress, client.Port);
                    s.SendTo(sendbuf, ep);
                    log.Info("Sent '" + message + "' sent to " + client.ToString());
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    log.Error("Unable to send message '" + message + "' to " + client.ToString());
                    log.Warn("Removing client " + client.ToString() + " from notification list.");
                    _clientList.Remove(client);
                }
            }
        }
    }
}
