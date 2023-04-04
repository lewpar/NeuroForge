using NeuroForge.Shared.Network.Authentication;
using NeuroForge.Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroForge.Server.Network
{
    public class PacketHandlers
    {
        public static async Task<PacketHandlerResult> HandleAuthAsync(NeuroForgeUser user)
        {
            int packetSize = await NetworkHelper.ReadInt32Async(user.Stream);
            byte[] data = await NetworkHelper.ReadBytesAsync(user.Stream, packetSize);

            var userCreds = NetworkHelper.Deserialize<UserCredentials>(data);
            if (userCreds == null)
            {
                return new PacketHandlerResult(false, "Malformed packet data.");
            }

            // TODO: Replace with DB call.
            if(userCreds.Username != "username" ||
                userCreds.HashedPassword != "password")
            {
                return new PacketHandlerResult(false, "Invalid user credentials.");
            }

            /*var command = MySQL.Connection.CreateCommand();
            command.CommandText = "SELECT username, password FROM accounts WHERE username = @Username AND password = @Password;";
            command.Parameters.AddWithValue("Username", userCreds.Username);
            command.Parameters.AddWithValue("Password", userCreds.HashedPassword);

            using var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                return false;
            }*/

            string result = "OK";
            data = Encoding.UTF8.GetBytes(result);

            await NetworkHelper.WriteInt32Async(user.Stream, (int)PacketType.Auth);
            await NetworkHelper.WriteInt32Async(user.Stream, data.Length);
            await NetworkHelper.WriteBytesAsync(user.Stream, data);

            return new PacketHandlerResult(true, string.Empty);
        }
    }
}
