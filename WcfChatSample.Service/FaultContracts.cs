using System.Runtime.Serialization;

namespace WcfChatSample.Service
{
    [DataContractAttribute]
    public class UserKeyFault
    {
        public UserKeyFault()
        {
        }
    }

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
    public class ServerInternalFault
    {
        public ServerInternalFault()
        {
        }
    }
}
