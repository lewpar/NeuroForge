using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NeuroForge.Server.Network.Events;
using NeuroForge.Server.Network.Exceptions;

namespace NeuroForge.Server.Network
{
    public class NeuroForgeServer
    {
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<EventArgs>? ServerStarted;

        private TcpListener _listener;

        private CancellationToken _exitToken;
        private CancellationTokenSource _exitTokenSrc;

        private X509Certificate2? _sslCertificate;

        private List<TcpClient> _connectedClients;

        public NeuroForgeServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _exitTokenSrc = new CancellationTokenSource();
            _exitToken = _exitTokenSrc.Token;

            _connectedClients = new List<TcpClient>();
        }

        public async Task LoadCertificateAsync(StoreName storeName, StoreLocation storeLoc, string certName)
        {
            await Task.Run(() =>
            {
                X509Store store = new X509Store(storeName, storeLoc);
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, certName, false);
                _sslCertificate = certs[0];
            });
        }

        public async Task ListenAsync()
        {
            if(_sslCertificate == null)
            {
                throw new CertificateConfigurationException("SSLCertificate was null!");
            }

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

        private async Task DiconnectClientAsync(TcpClient client)
        {
            client.Close();
        }

        private async void HandleClientAsync(TcpClient client)
        {
            if(!await HandshakeAsync(client))
            {
                await DiconnectClientAsync(client);
                return;
            }

            OnClientConnected(new ClientConnectedEventArgs(client));

            Console.WriteLine("Passed handshake.");
        }

        private async Task<bool> HandshakeAsync(TcpClient client)
        {
            var sslStream = new SslStream(client.GetStream(), false);

            try
            {
                if (_sslCertificate == null)
                {
                    throw new CertificateConfigurationException("SSLCertificate was null!");
                }

                await sslStream.AuthenticateAsServerAsync(
                    serverCertificate: _sslCertificate, 
                    clientCertificateRequired: false, 
                    enabledSslProtocols: SslProtocols.None, 
                    checkCertificateRevocation: true);

                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;

                _connectedClients.Add(client);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
