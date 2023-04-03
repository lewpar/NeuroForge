using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroForge.Shared.Network.Authentication
{
    public class UserCredentials
    {
        public string Username { get; private set; }
        public string HashedPassword { get; private set; }

        public UserCredentials(string username, string hashedPassword)
        {
            this.Username = username;
            this.HashedPassword = hashedPassword;
        }
    }
}
