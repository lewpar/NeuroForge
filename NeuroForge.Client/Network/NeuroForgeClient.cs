﻿using NeuroForge.Shared.Network;
using NeuroForge.Shared.Network.Authentication;
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
            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port);

                _sslStream = new SslStream(_client.GetStream(), false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
                await _sslStream.AuthenticateAsClientAsync("localhost");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                _sslStream.Close();
                _client.Close();
            });
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if(_sslStream == null || _client == null)
                {
                    return false;
                }

                await NetworkHelper.WriteInt32Async(_sslStream, (int)PacketType.TestConnection);

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            // Request
            var userCreds = new UserCredentials(username, password);
            var userCredsData = NetworkHelper.Serialize<UserCredentials>(userCreds);

            await NetworkHelper.WriteInt32Async(_sslStream, (int)PacketType.Auth);
            await NetworkHelper.WriteInt32Async(_sslStream, userCredsData.Length);
            await NetworkHelper.WriteBytesAsync(_sslStream, userCredsData);

            // Response
            PacketType packetType = (PacketType)await NetworkHelper.ReadInt32Async(_sslStream);
            if(packetType != PacketType.Auth)
            {
                return false;
            }

            int packetLength = await NetworkHelper.ReadInt32Async(_sslStream);
            byte[] packetData = await NetworkHelper.ReadBytesAsync(_sslStream, packetLength);

            string result = Encoding.UTF8.GetString(packetData);

            if(result != "OK")
            {
                return false;
            }

            return true;
        }
    }
}
