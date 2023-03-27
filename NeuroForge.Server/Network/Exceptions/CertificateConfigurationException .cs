using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroForge.Server.Network.Exceptions
{
    public class CertificateConfigurationException : Exception
    {
        public CertificateConfigurationException(string? message) : base(message) { }
    }
}
