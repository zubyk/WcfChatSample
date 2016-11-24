using System;
using System.Security;

namespace WcfChatSample.Client.Wpf.Data
{
    internal class UserLogin
    {
        public string Username { get; private set; }
        public SecureString SecurePassword { get; private set; }
        public bool IsSelf { get; private set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Username) && SecurePassword != null && SecurePassword.Length > 0; 
            }
        }

        public UserLogin(string username, SecureString password)
        {
            if (String.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            Username = username;
            SecurePassword = password;
        }

        public UserLogin(string username, bool isSelf)
            : this(username, null)
        {
            Username = username;
            IsSelf = isSelf;
        }
    }
}
