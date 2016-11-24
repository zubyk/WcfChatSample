using System.Runtime.Serialization;

namespace WcfChatSample.Service
{
    [DataContractAttribute]
    public class UserLoginFault
    {
        public UserLoginFault()
        {
        }
    }

    [DataContractAttribute]
    public class UserSessionTimeoutFault
    {
        public UserSessionTimeoutFault()
        {
        }
    }

    [DataContractAttribute]
    public class UserLoginRequiredFault
    {
        public UserLoginRequiredFault()
        {
        }
    }

    [DataContractAttribute]
    public class ServerInternalFault
    {
        public ServerInternalFault()
        {
        }
    }
}
