using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfChatSample.Client.Wpf.ServiceReference
{
    public class ChatServiceCallback : IChatServiceCallback
    {
        public event EventHandler<ChatMessage[]> MessagePost = delegate { };
        public event EventHandler<string[]> UsersListChange = delegate { };

        public void OnMessagePost(ChatMessage[] message)
        {
            MessagePost(this, message);
        }

        public void OnUsersListChange(string[] users)
        {
            UsersListChange(this, users);
        }
    }
}
