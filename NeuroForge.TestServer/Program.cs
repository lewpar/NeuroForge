using System.Security.Cryptography.X509Certificates;

using NeuroForge.Server.Network;
using NeuroForge.Server.Network.Events;

namespace NeuroForge.TestServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var nfServer = new NeuroForgeServer(System.Net.IPAddress.Any, 4411);

            nfServer.ClientConnected += NfServer_ClientConnected;
            nfServer.ClientAuthenticated += NfServer_ClientAuthenticated;
            nfServer.ServerStarted += NfServer_ServerStarted;

            try
            {
                await nfServer.LoadCertificateAsync(StoreName.My, StoreLocation.CurrentUser, "localhost");
                Console.WriteLine($"Found certificate: {nfServer.SslCertificate?.FriendlyName}");
                Console.WriteLine($"{nfServer.SslCertificate?.ToString()}");
                await nfServer.ListenAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("The application encountered an unexpected error:");
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }

        private static void NfServer_ClientAuthenticated(object? sender, ClientConnectedEventArgs e)
        {
            Console.WriteLine($"Client '{e.User.Client.Client.RemoteEndPoint}' authenticated.");
        }

        private static void NfServer_ServerStarted(object? sender, EventArgs e)
        {
            Console.WriteLine("Server started, listening for clients..");
        }

        private static void NfServer_ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            Console.WriteLine($"Client '{e.User.Client.Client.RemoteEndPoint}' connected.");
        }
    }
}
