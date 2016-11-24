using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WcfChatSample.Service
{
    partial class ChatService : IChatService
    {
        private static IDbProvider _db = null;
        private static List<UserCredentials> _creds = new List<UserCredentials>();
        private static object _creds_lock = new object();

        private UserCredentials _user;

        private void Log(string message, params object[] args)
        {
            LogMessage(this, args != null && args.Length > 0 ? String.Format(message, args) : message);
        }

        private void ExecuteAction<T>(Action<UserCredentials, T> a, T arg)
        {
            if (a != null)
            {
                var creds = _creds.ToArray();

                if (creds.Any())
                {
                    Task.Factory.StartNew(() =>
                    {
                        Parallel.ForEach(creds, (cred) =>
                        {
                            a.Invoke(cred, arg);
                        });
                    });
                }
            }
        }

        private void SendUserlist(string[] userlist)
        {
            if (userlist != null && userlist.Length > 0)
            {
                ExecuteAction<string[]>((cred, arg) =>
                {
                    try
                    {
                        if ((((ICommunicationObject)cred.Callback).State == CommunicationState.Opened))
                        {
                            cred.Callback.OnUsersListChange(arg);
                        }
                    }
                    catch (Exception e)
                    {
                        Log("Callback OnUsersListChange() error for user {0}: {1}", cred, e.Message);
                    }
                }, userlist);
            }
        }

        private void SendMessage(string message, params object[] args)
        {
            SendMessage(new ChatMessage() { 
                Username = null
                , Date = DateTime.Now
                , Text = args != null && args.Length > 0 ? String.Format(message, args) : message
            });
        }

        private void SendMessage(ChatMessage msg)
        {
            if (msg != null)
            {
                ExecuteAction<ChatMessage>((cred, arg) =>
                {
                    try
                    {
                        if ((((ICommunicationObject)cred.Callback).State == CommunicationState.Opened))
                        {
                            cred.Callback.OnMessagePost(new ChatMessage[] { arg });
                        }
                    }
                    catch (Exception e)
                    {
                        Log("Callback OnMessagePost() error for user {0}: {1}", cred, e.Message);
                    }
                }, msg);
            }
        }

        private void ValidateSession([CallerMemberName] string source = null)
        {
            if (_user == null)
            {
                Log("{0}: User not logined", source);
                throw new FaultException<UserLoginRequiredFault>(new UserLoginRequiredFault());
            }
            else if (!_creds.Contains(_user))
            {
                Log("{0}: User {1} session timeout", source, _user);
                throw new FaultException<UserSessionTimeoutFault>(new UserSessionTimeoutFault()); 
            }
            else 
            {
                switch (_user.Channel.State)
                {
                    case CommunicationState.Closed:    
                    case CommunicationState.Closing:
                    case CommunicationState.Faulted:
                        Log("{0}: User {1} session timeout", source, _user);
                        throw new FaultException<UserSessionTimeoutFault>(new UserSessionTimeoutFault());
                }
            }
        }

        private void ConnectionClosed(object sender, EventArgs e)
        {
            if (_user != null)
            {
                bool contains = false;

                lock (_creds_lock)
                {
                    contains = _creds.Remove(_user);
                }
                
                if (contains)
                {
                    SendUserlist(_creds.Select(u => u.Username).ToArray());
                    SendMessage("User '{0}' disconnected", _user.Username);
                    Log("User {0} disconnected", _user);
                }
            }
        }
    }
}
