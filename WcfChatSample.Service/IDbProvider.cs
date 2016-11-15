using System;
using System.Collections.Generic;

namespace WcfChatSample.Service
{
    public enum LoginResult : byte
    {
        None,
        User,
        Admin
    }
    
    public interface IDbProvider
    {
        bool AddMessage(IDbMessage message);

        IDbMessage[] GetLastMessages(int? count);

        LoginResult Login(string username, string password);
    }

    public interface IDbMessage
    {
        DateTime Date { get; }
        string Username { get; }
        string Message { get; }
    }
}
