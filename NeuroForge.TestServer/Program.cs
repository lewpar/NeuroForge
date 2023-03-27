using NeuroForge.Server.Network;

namespace NeuroForge.TestServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var nfServer = new NeuroForgeServer(System.Net.IPAddress.Any, 4411);

            nfServer.ClientConnected += NfServer_ClientConnected;
            nfServer.ServerStarted += NfServer_ServerStarted;

            await nfServer.ListenAsync();

            Console.ReadLine();
        }

        private static void NfServer_ServerStarted(object? sender, EventArgs e)
        {
            Console.WriteLine("Server started, listening for clients..");
        }

        private static void NfServer_ClientConnected(object? sender, Server.Network.Event.ClientConnectedEventArgs e)
        {
            Console.WriteLine($"Client '{e.Client.Client.RemoteEndPoint}' connected.");
        }
    }
}
