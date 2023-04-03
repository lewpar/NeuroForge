using NeuroForge.Client.Network;
using System.Net;

namespace NeuroForge.TestClient
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new NeuroForgeClient(IPAddress.Loopback, 4411);
            await client.ConnectAsync();
            Console.WriteLine("Connected to server.");
            await client.AuthenticateAsync();
            Console.ReadLine();
        }
    }
}
