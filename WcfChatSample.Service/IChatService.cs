using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
        [FaultContractAttribute(typeof(UserKeyFault))]
        [FaultContractAttribute(typeof(UserSessionTimeoutFault))]
        [FaultContractAttribute(typeof(ServerInternalFault))]
        void Post(string key, string message);
        
        [OperationContract]
        [FaultContractAttribute(typeof(UserKeyFault))]
        [FaultContractAttribute(typeof(UserSessionTimeoutFault))]
        void DisconnectUser(string key, string username);

        [OperationContract]
        [FaultContractAttribute(typeof(UserKeyFault))]
        [FaultContractAttribute(typeof(UserSessionTimeoutFault))]
        [FaultContractAttribute(typeof(ServerInternalFault))]
        ChatMessage[] Refresh(string key);
    }

    public interface IChatServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessagePost(ChatMessage[] message);

        [OperationContract(IsOneWay = true)]
        void OnUsersListChange(string[] users);

        [OperationContract(IsOneWay = true)]
        void OnDisconnect();
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
        [DataMember]
        public bool IsAdmin { get; set; }

        [DataMember]
        public string Key { get; set; }

        internal IChatServiceCallback Callback { get; set; }

        internal string Username { get; set; }
    }
}
