using System;
using System.Security;

namespace WcfChatSample.Client.Wpf.Data
{
    public class UserLogin
    {
        public string Username { get; private set; }
        public SecureString SecurePassword { get; private set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Username) && SecurePassword != null && SecurePassword.Length > 0; 
            }
        }

        public UserLogin(string username, SecureString password)
        {
            Username = username;
            SecurePassword = password;
        }
    }
}
