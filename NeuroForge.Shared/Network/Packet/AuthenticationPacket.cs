using NeuroForge.Shared.Network.Authentication;

namespace NeuroForge.Shared.Network.Packet
{
    public class AuthenticationPacket : IPacket
    {
        public PacketType Type { get; private set; }

        public int Length { get; private set; }

        public UserCredentials Credentials;

        public AuthenticationPacket(int length, string username, string hashedPassword)
        {
            this.Type = PacketType.Auth;
            this.Length = length;

            this.Credentials = new UserCredentials(username, hashedPassword);
        }
    }
}
