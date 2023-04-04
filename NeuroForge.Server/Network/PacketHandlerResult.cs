using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroForge.Server.Network
{
    public class PacketHandlerResult
    {
        public bool Result { get; private set; }
        public string Message { get; private set; }

        public PacketHandlerResult(bool result, string message)
        {
            this.Result = result;
            this.Message = message;
        }
    }
}
