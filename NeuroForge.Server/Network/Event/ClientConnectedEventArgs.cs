using System.Net.Sockets;

namespace NeuroForge.Server.Network.Event
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public TcpClient Client { get; private set; }

        public ClientConnectedEventArgs(TcpClient client)
        {
            this.Client = client;
        }
    }
}
