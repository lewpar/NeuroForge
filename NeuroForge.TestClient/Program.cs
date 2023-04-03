using NeuroForge.Client.Network;
using System.Net;

namespace NeuroForge.TestClient
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new NeuroForgeClient(IPAddress.Parse("127.0.0.1"), 4411);

            Console.WriteLine("Connecting to server..");
            await client.ConnectAsync();
            Console.WriteLine("Connected.");

            string username = "username123";
            string password = "password123";

            Console.WriteLine($"Authenticating with {username} : {password}..");
            if(!await client.AuthenticateAsync(username, password))
            {
                Console.WriteLine("Failed to authenticate.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Authenticated.");

            Console.ReadLine();
        }
    }
}
