using NeuroForge.Client.Network;
using System.Net;

namespace NeuroForge.TestClient
{
    internal class Program
    {
        static NeuroForgeClient client;

        public static async Task Main(string[] args)
        {
            client = new NeuroForgeClient(IPAddress.Parse("127.0.0.1"), 4411);

            await HandleInputAsync();

            Console.ReadLine();
        }

        public static async Task HandleInputAsync()
        {
            while(true)
            {
                Console.Clear();
                Console.WriteLine("1) Register");
                Console.WriteLine("2) Login");
                Console.WriteLine("3) Quit");
                Console.Write("> ");
                var input = Console.ReadLine();

                switch(input.ToLower().Trim())
                {
                    case "1":
                        await RegisterAsync();
                        break;

                    case "2":
                        await LoginAsync();
                        break;

                    case "q":
                        Environment.Exit(0);
                        return;

                    default:
                        continue;
                }
            }
        }

        public static async Task RegisterAsync()
        {

        }

        public static async Task LoginAsync()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine().Trim();

            Console.Write("Password: ");
            string password = Console.ReadLine().Trim();

            // TODO: Remove this and track login state.
            if(await client.TestConnectionAsync())
            {
                await client.DisconnectAsync();
            }

            Console.WriteLine("Connecting to server..");
            if (!await client.ConnectAsync())
            {
                Console.WriteLine("Failed to connect.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Connected.");

            Console.WriteLine("Authenticating..");
            if (!await client.AuthenticateAsync(username, password))
            {
                Console.WriteLine("Failed to authenticate.");
                Console.Write("Press ENTER to go back to menu.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Logged in as '{username}'.");
            Console.ReadLine();
        }
    }
}
