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

        private List<NeuroForgeUser> _connectedUsers;

        public NeuroForgeServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _exitTokenSrc = new CancellationTokenSource();
            _exitToken = _exitTokenSrc.Token;

            _connectedUsers = new List<NeuroForgeUser>();
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

        private async Task DiconnectClientAsync(NeuroForgeUser user)
        {
            //TODO: Send disconnect packet.

            user.Stream.Close();
            user.Client.Close();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var user = new NeuroForgeUser(client);

            if(!await HandshakeAsync(user))
            {
                await DiconnectClientAsync(user);
                return;
            }

            if(!await AuthenticateAsync(user))
            {
                await DiconnectClientAsync(user);
                return;
            }

            OnClientConnected(new ClientConnectedEventArgs(user));

            _connectedUsers.Add(user);
        }

        private async Task<bool> HandshakeAsync(NeuroForgeUser user)
        {
            try
            {
                if (_sslCertificate == null)
                {
                    throw new CertificateConfigurationException("SSLCertificate was null!");
                }

                await user.Stream.AuthenticateAsServerAsync(
                    serverCertificate: _sslCertificate, 
                    clientCertificateRequired: false, 
                    enabledSslProtocols: SslProtocols.None, 
                    checkCertificateRevocation: true);

                user.Stream.ReadTimeout = 5000;
                user.Stream.WriteTimeout = 5000;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> AuthenticateAsync(NeuroForgeUser user)
        {
            return await Task.FromResult<bool>(true);
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
