using System;
using System.Linq;
using System.ServiceModel;

namespace WcfChatSample.Service
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession)]
    public partial class ChatService : IChatService
    {
        public static event EventHandler<string> LogMessage = delegate {};

        public static void Initialize(IDbProvider db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db", "Call ChatService.Initialize() with null argument");
            }

            _db = db;
        }

        public UserCredentials Login(string username, string password)
        {
            var result = LoginResult.None;
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
                _user = new UserCredentials()
                {
                    Callback = OperationContext.Current.GetCallbackChannel<IChatServiceCallback>(),
                    Username = username,
                    Role = result == LoginResult.Admin ? UserRole.Admin : UserRole.User,
                };

                _user.Channel.Closed += ConnectionClosed;
                _user.Channel.Faulted += ConnectionClosed;

                lock (_creds_lock)
                {
                    var oldUser = _creds.FirstOrDefault(u => u.Username == _user.Username);

                    if (oldUser != null)
                    {
                        _creds.Remove(oldUser);
                        try
                        {
                            oldUser.Channel.Close();
                        }
                        catch (Exception e)
                        {
                            Log("Error close user {0} connection: {1}", oldUser, e.Message);
                        }
                    }

                    _creds.Add(_user);
                }

                SendUserlist(_creds.Select(u => u.Username).ToArray());
                SendMessage("User '{0}' connected", _user.Username);

                Log("User {0} log in, role - {1}", _user, _user.Role);

                return _user;
            }
            else
            {
                Log("Error on login attempt, invalid password for registered user '{0}'", username);
                throw new FaultException<UserLoginFault>(new UserLoginFault());
            }
        }

        public void DisconnectUser(string username)
        {
            UserCredentials toDisconnect = null;

            lock (_creds_lock)
            {
                ValidateSession();

                if (_user.Role == UserRole.Admin)
                {
                    toDisconnect = _creds.FirstOrDefault(u => u.Username == username);

                    if (toDisconnect != null)
                    {
                        _creds.Remove(toDisconnect);
                    }
                }
            }

            if (toDisconnect != null)
            {
                Log("User {0} call DisconnectUser('{1}')", _user, username);

                var msg = new ChatMessage()
                {
                    Date = DateTime.Now,
                    Text = String.Format("User '{0}' disconnected by user '{1}'", toDisconnect.Username, _user.Username)
                };

                try
                {
                    toDisconnect.Callback.OnMessagePost(new ChatMessage[1] { msg });
                }
                catch (Exception e)
                {
                    Log("Callback OnMessagePost() error for user {0}: {1}", toDisconnect, e.Message); 
                }

                try
                {
                    toDisconnect.Channel.Abort();
                }
                catch (Exception e)
                {
                    Log("Error disconnect user {0}: {1}", toDisconnect, e.Message);
                }

                SendUserlist(_creds.Select(u => u.Username).ToArray());
                SendMessage(msg);

                Log("User {0} disconnected by user {1}", toDisconnect, _user);

            }
            else
            {
                Log("User {0} call DisconnectUser('{1}') canceled because {2}", _user.Username, username, _user.Role != UserRole.Admin ? "target user not found" : "user has not Admin role");  
            }
        }

        public void Post(string message)
        {
            ValidateSession();
            Log("User {0} call Post('{1}')", _user.Username, message);

            var msg = new ChatMessage()
            {
                Date = DateTime.Now,
                Text = message,
                Username = _user.Username
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

            SendMessage(msg);
        }

        public ChatMessage[] Refresh()
        {
            ValidateSession();
            Log("User '{0}' call Refresh()", _user.Username);

            try
            {
                var msgs = _db.GetLastMessages(_user.Role == UserRole.Admin ? null : (int?)10)
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

    }
}
