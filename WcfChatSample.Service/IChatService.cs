using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WcfChatSample.Service
{
    [ServiceContract(CallbackContract = typeof(IChatServiceCallback), SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        [OperationContract]
        [FaultContractAttribute(typeof(UserLoginFault))]
        [FaultContractAttribute(typeof(ServerInternalFault))]
        UserCredentials Login(string username, string password);

        [OperationContract]
        [FaultContractAttribute(typeof(UserLoginRequiredFault))]
        [FaultContractAttribute(typeof(UserSessionTimeoutFault))]
        [FaultContractAttribute(typeof(ServerInternalFault))]
        void Post(string message);
        
        [OperationContract]
        [FaultContractAttribute(typeof(UserLoginRequiredFault))]
        [FaultContractAttribute(typeof(UserSessionTimeoutFault))]
        void DisconnectUser(string username);

        [OperationContract]
        [FaultContractAttribute(typeof(UserLoginRequiredFault))]
        [FaultContractAttribute(typeof(UserSessionTimeoutFault))]
        [FaultContractAttribute(typeof(ServerInternalFault))]
        ChatMessage[] Refresh();
    }

    public interface IChatServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessagePost(ChatMessage[] message);

        [OperationContract(IsOneWay = true)]
        void OnUsersListChange(string[] users);
    }

    [DataContract]
    public class ChatMessage : IDbMessage
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Text { get; set; }
    }

    [DataContract]
    public class UserCredentials
    {
        private static int _lastID = 1;
        
        [DataMember]
        public UserRole Role { get; set; }

        internal string Username { get; set; }

        internal IChatServiceCallback Callback { get; set; }

        internal int ID { get; private set; }

        internal ICommunicationObject Channel
        {
            get
            {
                return Callback as ICommunicationObject;
            }
        }

        public UserCredentials()
        {
            ID = _lastID++;
        }

        public override string ToString()
        {
            return String.Format("#{0} '{1}'", ID, Username);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var usr = obj as UserCredentials;

            return usr != null ? usr.ID == ID : false;
        }
    }

    public enum UserRole : byte
    {
        None,
        User,
        Admin
    }
}
