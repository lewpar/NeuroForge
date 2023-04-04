using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NeuroForge.Server.Database;
using NeuroForge.Server.Network.Events;
using NeuroForge.Server.Network.Exceptions;
using NeuroForge.Shared.Network;
using NeuroForge.Shared.Network.Authentication;

namespace NeuroForge.Server.Network
{
    public class NeuroForgeServer
    {
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientConnectedEventArgs>? ClientAuthenticated;
        public event EventHandler<EventArgs>? ServerStarted;

        private TcpListener _listener;

        private CancellationToken _exitToken;
        private CancellationTokenSource _exitTokenSrc;

        public X509Certificate2? SslCertificate { get; private set; }
        public MySQLService MySQL { get; private set; }

        private List<NeuroForgeUser> _connectedUsers;

        private Dictionary<PacketType, Func<NeuroForgeServer, NeuroForgeUser, Task<PacketHandlerResult>>> _packetHandlers;

        public NeuroForgeServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);

            _exitTokenSrc = new CancellationTokenSource();
            _exitToken = _exitTokenSrc.Token;

            _connectedUsers = new List<NeuroForgeUser>();

            MySQL = new MySQLService();

            _packetHandlers = new Dictionary<PacketType, Func<NeuroForgeServer, NeuroForgeUser, Task<PacketHandlerResult>>>
            {
                { PacketType.Auth, PacketHandlers.HandleAuthAsync }
            };
        }

        public async Task LoadCertificateAsync(StoreName storeName, StoreLocation storeLoc, string certName)
        {
            await Task.Run(() =>
            {
                X509Store store = new X509Store(storeName, storeLoc);
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, certName, false);
                SslCertificate = certs[0];
            });
        }

        public async Task ListenAsync()
        {
            if(SslCertificate == null)
            {
                throw new CertificateConfigurationException("SSLCertificate was null!");
            }

            _listener.Start();

            OnServerStarted(new EventArgs());

            while (!_exitToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                HandleClientAsync(client); // Fire and Forget
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

        private async Task DisconnectUserAsync(NeuroForgeUser user, string reason)
        {
            byte[] data = Encoding.UTF8.GetBytes(reason);

            await NetworkHelper.WriteInt32Async(user.Stream, (int)PacketType.Disconnect);
            await NetworkHelper.WriteInt32Async(user.Stream, data.Length);
            await NetworkHelper.WriteBytesAsync(user.Stream, data);

            user.Stream.Close();
            user.Client.Close();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var user = new NeuroForgeUser(client);
            OnClientConnected(new ClientConnectedEventArgs(user));

            if (!await HandshakeAsync(user))
            {
                await DisconnectUserAsync(user, "Failed SSL handshake.");
                return;
            }

            await HandleUserMessagesAsync(user);
        }

        private async Task HandleUserMessagesAsync(NeuroForgeUser user)
        {
            while(!_exitToken.IsCancellationRequested)
            {
                PacketType packetType = (PacketType)await NetworkHelper.ReadInt32Async(user.Stream);

                PacketHandlerResult result = await _packetHandlers[packetType].Invoke(this, user);

                if(!result.Result)
                {
                    await DisconnectUserAsync(user, result.Message);
                    return;
                }
            }
        }

        private async Task<bool> HandshakeAsync(NeuroForgeUser user)
        {
            try
            {
                if (SslCertificate == null)
                {
                    throw new CertificateConfigurationException("SSLCertificate was null!");
                }

                await user.Stream.AuthenticateAsServerAsync(
                    serverCertificate: SslCertificate, 
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

        private void OnClientConnected(ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        private void OnClientAuthenticated(ClientConnectedEventArgs e)
        {
            ClientAuthenticated?.Invoke(this, e);
        }

        private void OnServerStarted(EventArgs e)
        {
            ServerStarted?.Invoke(this, e);
        }
    }
}
