using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfChatSample.Client.Wpf.Data
{
    internal class UserLogin
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Username) && !String.IsNullOrWhiteSpace(Password); 
            }
        }

        public UserLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
