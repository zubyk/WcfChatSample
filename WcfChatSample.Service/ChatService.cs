using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
        }
        
        public ChatService()
        {
            _creds = new Dictionary<string, UserCredentials>();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);

                try
                {
                    List<UserCredentials> creds = null;
                    lock (_creds_lock)
                    {
                        foreach (var crd in _creds.Values.ToArray())
                        {
                            switch (((ICommunicationObject)crd.Callback).State)
                            {
                                case CommunicationState.Closed:
                                case CommunicationState.Faulted:
                                case CommunicationState.Closing:
                                    _creds.Remove(crd.Key);
                                    break;
                            }
                        }

                        creds = _creds.Values.ToList();
                    }

                    var usrs = creds.Select(c => c.Username).ToArray();

                    foreach (var crd in creds)
                    {
                        crd.Callback.OnUsersListChange(usrs);
                    }
                }
                catch (Exception e)
                {
                    Log("Error on userlist update: {0}", e.Message);
                }
            });
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
                    creds = new UserCredentials() { Key = Guid.NewGuid().ToString(), Username = username, IsAdmin = result == LoginResult.Admin, Callback = callback };

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
                Text = message,
                Username = user.Username
            };

            try 
            {
                _db.AddMessage(msg);
                Log("User \"{0}\" post new message", user.Username);
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
                                Text = msg.Text,
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

                if (String.IsNullOrWhiteSpace(username))
                {
                    username = user.Username;
                }
                
                if (user.IsAdmin || user.Username == username)
                {
                    toDisconnect = _creds.Values.FirstOrDefault(crd => crd.Username == username);

                    if (toDisconnect != null)
                    {
                        _creds.Remove(toDisconnect.Key);
                    }

                    creds = _creds.Values.ToList();
                }
            }
            if (creds == null)
            {
                Log("Error attempt of disconnect user \"{0}\" by user \"{1}\"", username, user.Username);
            }
            else
            {
                if (user.Username == username)
                {
                    Log("User \"{0}\" disconnected", user.Username);
                }
                else
                {
                    Log("User \"{0}\" disconnect user \"{1}\"", user.Username, username);
                }

                if (creds.Any())
                {
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
            }
        }

        public ChatMessage[] Refresh(string key)
        {
            UserCredentials user = null;
            lock (_creds_lock)
            {
                user = GetUserByKey(key);
            }

            Log("User \"{0}\" requested data refresh", user.Username);

            try
            {
                var msgs = _db.GetLastMessages(user.IsAdmin ? null : (int?)10)
                    .Select(msg => new ChatMessage() { Date = msg.Date, Text = msg.Text, Username = msg.Username })
                    .ToArray();

                return msgs;
            }
            catch (Exception e)
            {
                Log("DB error: {0}", e.Message);
                throw new FaultException<ServerInternalFault>(new ServerInternalFault());
            }
        }

        public string[] RefreshUsers(string key)
        {
            UserCredentials user = null;
            lock (_creds_lock)
            {
                user = GetUserByKey(key);
            }

            Log("User \"{0}\" requested userlist refresh", user.Username);

            try
            {
                var usrs = _creds.Values.Select(crd => crd.Username).ToArray();
                return usrs;
            }
            catch (Exception e)
            {
                Log("Error processing userlist: {0}", e.Message);
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
