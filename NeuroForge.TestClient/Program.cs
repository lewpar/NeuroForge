using NeuroForge.Client.Network;

namespace NeuroForge.TestClient
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new NeuroForgeClient(System.Net.IPAddress.Parse("127.0.0.1"), 4411);
            await client.ConnectAsync();
            Console.WriteLine("Connected to server.");
            Console.ReadLine();
        }
    }
}
