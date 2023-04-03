using System.Net.Security;
using System.Net.Sockets;

namespace NeuroForge.Server.Network
{
    public class NeuroForgeUser
    {
        public TcpClient Client { get; private set; }
        public SslStream Stream { get; private set; }

        public NeuroForgeUser(TcpClient client)
        {
            this.Client = client;
            this.Stream = new SslStream(client.GetStream(), false);
        }
    }
}
