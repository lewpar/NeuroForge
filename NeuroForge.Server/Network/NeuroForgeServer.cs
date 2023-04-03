using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
            OnClientConnected(new ClientConnectedEventArgs(user));

            if (!await HandshakeAsync(user))
            {
                await DiconnectClientAsync(user);
                return;
            }

            if(!await AuthenticateAsync(user)) 
            {
                await DiconnectClientAsync(user);
                return;
            }

            OnClientAuthenticated(new ClientConnectedEventArgs(user));

            _connectedUsers.Add(user);

            await HandleUserMessageAsync(user);
        }

        private async Task HandleUserMessageAsync(NeuroForgeUser user)
        {
            Console.WriteLine("Waiting for messages..");

            while(!_exitToken.IsCancellationRequested)
            {
                PacketType packetType = (PacketType)await NetworkHelper.ReadInt32Async(user.Stream);
                int packetSize = await NetworkHelper.ReadInt32Async(user.Stream);

                byte[] data = await NetworkHelper.ReadBytesAsync(user.Stream, packetSize);

                // TODO: Remove this and put a packet handler here.
                Console.WriteLine($"Got packet {packetType} with size {packetSize} and data {Encoding.UTF8.GetString(data)}");
            }
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
            PacketType packetType = (PacketType)await NetworkHelper.ReadInt32Async(user.Stream);
            if (packetType != PacketType.Auth)
            {
                return false;
            }

            int packetSize = await NetworkHelper.ReadInt32Async(user.Stream);
            byte[] data = await NetworkHelper.ReadBytesAsync(user.Stream, packetSize);

            var userCreds = NetworkHelper.Deserialize<UserCredentials>(data);
            if(userCreds == null)
            {
                return false;
            }

            // TODO: Replace with DB auth.
            if(userCreds.Username != "username" ||
                userCreds.HashedPassword != "password")
            {
                return false;
            }

            string result = "OK";
            data = Encoding.UTF8.GetBytes(result);
            await NetworkHelper.WriteInt32Async(user.Stream, (int)PacketType.Auth);
            await NetworkHelper.WriteInt32Async(user.Stream, data.Length);
            await NetworkHelper.WriteBytesAsync(user.Stream, data);

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
