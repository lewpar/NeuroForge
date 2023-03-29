using System.Net.Sockets;

namespace NeuroForge.Server.Network.Events
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public NeuroForgeUser User { get; private set; }

        public ClientConnectedEventArgs(NeuroForgeUser user)
        {
            this.User = user;
        }
    }
}
