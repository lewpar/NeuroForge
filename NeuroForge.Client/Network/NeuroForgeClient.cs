using NeuroForge.Shared.Network;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NeuroForge.Client.Network
{
    public class NeuroForgeClient
    {
        private TcpClient _client;
        private SslStream _sslStream;

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

            _sslStream = new SslStream(_client.GetStream(), false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
            await _sslStream.AuthenticateAsClientAsync("localhost");
        }

        public async Task AuthenticateAsync()
        {
            // TODO: Remove this an add real authentication logic here.
            Console.WriteLine("Sending message..");

            string message = "Hello World!";
            byte[] data = Encoding.UTF8.GetBytes(message);

            await NetworkHelper.WriteInt32Async(_sslStream, (int)PacketType.Auth);
            await NetworkHelper.WriteInt32Async(_sslStream, data.Length);
            await NetworkHelper.WriteBytesAsync(_sslStream, data);
        }
    }
}
