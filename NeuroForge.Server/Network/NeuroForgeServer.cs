using NeuroForge.Server.Network.Event;
using System.Net;
using System.Net.Sockets;

namespace NeuroForge.Server.Network
{
    public class NeuroForgeServer
    {
        public event EventHandler<NeuroClientConnectedEventArgs> ClientConnected;

        private TcpListener _listener;

        private CancellationToken _exitToken;
        private CancellationTokenSource _exitTokenSrc;

        public NeuroForgeServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _exitTokenSrc = new CancellationTokenSource();
            _exitToken = _exitTokenSrc.Token;
        }

        public async Task Listen()
        {
            _listener.Start();

            while(!_exitToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                HandleClient(client);
            }
        }

        public async Task Stop()
        {
            await Task.Run(() =>
            {
                _exitTokenSrc.Cancel();
                _listener.Stop();
            });
        }

        private async void HandleClient(TcpClient client)
        {
            OnClientConnected(new NeuroClientConnectedEventArgs(client));
        }

        private void OnClientConnected(NeuroClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }
    }
}
