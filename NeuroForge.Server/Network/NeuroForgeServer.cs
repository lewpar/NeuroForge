using System.Net;
using System.Net.Sockets;

using NeuroForge.Server.Network.Event;

namespace NeuroForge.Server.Network
{
    public class NeuroForgeServer
    {
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<EventArgs>? ServerStarted;

        private TcpListener _listener;

        private CancellationToken _exitToken;
        private CancellationTokenSource _exitTokenSrc;

        public NeuroForgeServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _exitTokenSrc = new CancellationTokenSource();
            _exitToken = _exitTokenSrc.Token;
        }

        public async Task ListenAsync()
        {
            _listener.Start();

            OnServerStarted(new EventArgs());

            while (!_exitToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                HandleClientAsync(client);
            }
        }

        public async Task StopAsync()
        {
            await Task.Run(() =>
            {
                _exitTokenSrc.Cancel();
                _listener.Stop();
            });
        }

        private async void HandleClientAsync(TcpClient client)
        {
            OnClientConnected(new ClientConnectedEventArgs(client));
        }

        private void OnClientConnected(ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        private void OnServerStarted(EventArgs e)
        {
            ServerStarted?.Invoke(this, e);
        }
    }
}
