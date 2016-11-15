using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WcfChatSample.Service
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private static IDbProvider _db;
        private static Dictionary<string, UserCredentials> _creds;
        private static object _creds_lock = new object();

        public static event EventHandler<string> LogMessage = delegate {};

        public static void Initialize(IDbProvider db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db", "Call ChatService.Initialize() with null argument");
            }

            _db = db;
            _creds = new Dictionary<string, UserCredentials>();
        }
        
        public UserCredentials Login(string username, string password)
        {
            LoginResult result = LoginResult.None;
            try
            {
                result = _db.Login(username, password);
            }
            catch (Exception e)
            {
                Log("DB error: {0}", e.Message);
                throw new FaultException<ServerInternalFault>(new ServerInternalFault());
            }
                        
            if (result != LoginResult.None)
            {
                UserCredentials creds = null;
                lock (_creds_lock)
                {
                    creds = _creds.Values.FirstOrDefault(crd => crd.Username == username);

                    if (creds != null)
                    {
                        _creds.Remove(creds.Key);
                    }

                    var callback = OperationContext.Current.GetCallbackChannel<IChatServiceCallback>();
                    creds = new UserCredentials() { Key = Guid.NewGuid().ToString(), IsAdmin = result == LoginResult.Admin, Callback = callback };

                    _creds.Add(creds.Key, creds);
                }

                Log("User \"{0}\" log in, role - {1}, key - {2}", username, result, creds.Key);

                return creds;
            }
            else
            {
                Log("Error on login attempt, invalid password for registered user \"{0}\"", username);
                throw new FaultException<UserLoginFault>(new UserLoginFault());
            }
        }

        public void Post(string key, string message)
        {
            UserCredentials user = null;
            List<UserCredentials> creds = null;
            lock (_creds_lock)
            {
                user = GetUserByKey(key);
                creds = _creds.Values.ToList();
            }

            var msg = new ChatMessage()
            {
                Date = DateTime.Now,
                Message = message,
                Username = user.Username
            };

            try 
            {
                _db.AddMessage(msg);
            }
            catch (Exception e)
            {
                Log("DB error: {0}", e.Message);
                throw new FaultException<ServerInternalFault>(new ServerInternalFault());
            }

            Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(creds
                , () =>
                    {
                        return new ChatMessage()
                            {
                                Date = msg.Date,
                                Message = msg.Message,
                                Username = msg.Username
                            };
                    }
                , (cred, loopState, m) =>
                    {
                        try
                        {
                            if ((((ICommunicationObject)cred.Callback).State == CommunicationState.Opened))
                            {
                                cred.Callback.OnMessagePost(new ChatMessage[] { m });
                            }
                        }
                        catch (Exception e)
                        {
                            Log("Callback OnMessagePost() error for user \"{0}\": {1}", cred.Username, e.Message);
                        }

                        return m;
                    }
                , _ => {});
            });
        }

        public void DisconnectUser(string key, string username)
        {
            UserCredentials user = null;
            UserCredentials toDisconnect = null;
            List<UserCredentials> creds = null;
            lock (_creds_lock)
            {
                user = GetUserByKey(key);
                creds = _creds.Values.ToList();

                toDisconnect = creds.FirstOrDefault(crd => crd.Username == username);

                if (toDisconnect != null)
                {
                    _creds.Remove(toDisconnect.Key);
                }
            }

            try
            {
                if (toDisconnect != null)
                {
                    creds.RemoveAll(crd => crd.Username == username);

                    if ((((ICommunicationObject)toDisconnect.Callback).State == CommunicationState.Opened))
                    {
                        toDisconnect.Callback.OnDisconnect();
                    }
                }
            }
            catch (Exception e)
            {
                Log("Callback OnDisconnect() error for user \"{0}\": {1}", toDisconnect.Username, e.Message);
            }

            Task.Factory.StartNew(() =>
            {
                var users = creds.Where(crd => ((ICommunicationObject)crd.Callback).State == CommunicationState.Opened && crd.Username != username)
                    .Select(c => c.Username).ToArray();

                Parallel.ForEach(creds.Where(crd => users.Contains(crd.Username))
                , () =>
                    {
                        return (string[])users.Clone();
                    }
                , (cred, loopState, usrs) =>
                    {
                        try
                        {
                            cred.Callback.OnUsersListChange(usrs);
                        }
                        catch (Exception e)
                        {
                            Log("Callback OnUsersListChange() error for user \"{0}\": {1}", cred.Username, e.Message);
                        }

                        return usrs;
                    }
                , (usrs) => { usrs = null; });
            });
        }

        public ChatMessage[] Refresh(string key)
        {
            UserCredentials user = null;
            lock (_creds_lock)
            {
                user = GetUserByKey(key);
            }
            
            try
            {
                var msgs = _db.GetLastMessages(user.IsAdmin ? null : (int?)10)
                    .Select(msg => new ChatMessage() { Date = msg.Date, Message = msg.Message, Username = msg.Username })
                    .ToArray();

                return msgs;
            }
            catch (Exception e)
            {
                Log("DB error: {0}", e.Message);
                throw new FaultException<ServerInternalFault>(new ServerInternalFault());
            }
        }

        private UserCredentials GetUserByKey(string key, [CallerMemberName] string source = null)
        {
            var user = _creds[key];

            if (user == null)
            {
                Log("{0}: User with key '{1}' not found", source, key);
                throw new FaultException<UserKeyFault>(new UserKeyFault());
            }
            else if (((ICommunicationObject)user.Callback).State != CommunicationState.Opened)
            {
                _creds.Remove(user.Key);
                
                Log("{0}: User`s with key '{1}' session timeout", source, key);
                throw new FaultException<UserSessionTimeoutFault>(new UserSessionTimeoutFault());
            }
            else
            {
                return user;
            }
        }

        private void Log(string message, params object[] args)
        {
            LogMessage(this, String.Format(message, args));
        }
    }
}
