using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfChatSample.Client.Wpf.Data
{
    public class UserEntity
    {
        public string Username { get; private set; }
        public bool IsSelf { get; private set; }

        public UserEntity(string username, bool self)
        {
            Username = username;
            IsSelf = self;
        }
    }
}
