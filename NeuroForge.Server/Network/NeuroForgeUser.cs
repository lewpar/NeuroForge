using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
