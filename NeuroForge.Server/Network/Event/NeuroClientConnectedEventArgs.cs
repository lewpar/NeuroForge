using System.Net.Sockets;

namespace NeuroForge.Server.Network.Event
{
    public class NeuroClientConnectedEventArgs : EventArgs
    {
        public TcpClient Client { get; private set; }

        public NeuroClientConnectedEventArgs(TcpClient client)
        {
            this.Client = client;
        }
    }
}
