using System;
namespace MCDBackend.Notificaitons
{
    public interface INotificationServer
    {
        void AddClient(ClientObject client);
        void RemoveClient(ClientObject client);
        void SendNotification(string message);
    }
}
