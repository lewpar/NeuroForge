using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace NeuroForge.Client.Network
{
    public class NeuroForgeClient
    {
        private TcpClient _client;

        private IPAddress _ipAddress;
        private int _port;

        public NeuroForgeClient(IPAddress ipAddress, int port)
        {
            _client = new TcpClient();

            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(_ipAddress, _port);

            var sslStream = new SslStream(_client.GetStream(), false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
            await sslStream.AuthenticateAsClientAsync("localhost");
        }
    }
}
