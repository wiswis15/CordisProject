
namespace MCDBackend.Notificaitons
{
    public class ClientObject
    {
        public string Address { get; set; }
        public int Port { get; set; }

        public ClientObject(string address, int port)
        {
            Address = address;
            Port = port;
        }

        override
        public string ToString()
        {
            return string.Format("Client: {0}:{1}", Address, Port);
        }
    }
}
