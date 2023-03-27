using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NeuroForge.Server.Network;

namespace NeuroForge.TestServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var nfServer = new NeuroForgeServer(System.Net.IPAddress.Any, 4411);
            nfServer.ClientConnected += NfServer_ClientConnected; ;
            await nfServer.Listen();

            Console.ReadLine();
        }

        private static void NfServer_ClientConnected(object? sender, Server.Network.Event.NeuroClientConnectedEventArgs e)
        {
            Console.WriteLine($"Client '{e.Client.Client.RemoteEndPoint}' connected.");
        }
    }
}
