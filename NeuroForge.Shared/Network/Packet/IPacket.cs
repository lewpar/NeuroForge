using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroForge.Shared.Network.Packet
{
    public interface IPacket
    {
        PacketType Type { get; }
        int Length { get; }
    }
}
