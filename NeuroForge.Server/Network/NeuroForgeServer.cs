using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

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

        public async Task LoadCertificateAsync(string certName)
        {
            await Task.Run(() =>
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
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

        private async void HandleClientAsync(TcpClient client)
        {
            OnClientConnected(new ClientConnectedEventArgs(client));

            if(!await HandshakeAsync(client))
            {
                Debug.WriteLine("An unexpected error occurred during client handshake.");
                return;
            }

            // TODO: Remove me
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

                // TODO: Server gets stuck here waiting for authentication from client.
                await sslStream.AuthenticateAsServerAsync(_sslCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;

                _connectedClients.Add(client);
            }
            catch (AuthenticationException ex)
            {
                sslStream.Close();
                client.Close();

                throw ex;
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
