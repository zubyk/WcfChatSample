using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Windows.Threading;
using WcfChatSample.Client.Wpf.ServiceReference;

namespace WcfChatSample.Client.Wpf.Data
{
    internal partial class DataSource : INotifyPropertyChanged
    {
        private ChatServiceClient _client;
        private ChatServiceCallback _callback;  

        private bool _isAsyncProcessing = false;
        private bool _isAdmin;
        private string _username;

        private object _messages_lock = new object();
        private object _users_lock = new object();

        public ObservableCollection<ChatMessage> Messages { get; private set; }
        public ObservableCollection<UserLogin> Users { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private ICommunicationObject _channel;

        private DataSource()
        {
            Messages = new ObservableCollection<ChatMessage>();
            Users = new ObservableCollection<UserLogin>();
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void callback_UsersListChange(object sender, string[] e)
        {
            lock (_users_lock)
            {
                Users = new ObservableCollection<UserLogin>(e.OrderBy(u => u)
                    .Where(u => !String.IsNullOrEmpty(u))
                    .Select(u => new UserLogin(u, u == _username)));
            }

            OnPropertyChanged("Users");
        }

        private void callback_MessagePost(object sender, ChatMessage[] e)
        {
            if (e.Length > 0)
            {
                lock (_messages_lock)
                {

                    AddMessages(e);
                }

                OnPropertyChanged("Messages");
            }
        }

        private void AddMessages(ChatMessage[] msgs)
        {
            if (Messages.Any())
            {
                foreach (var msg in msgs.OrderBy(m => m.Date))
                {
                    for (var i = Messages.Count - 1; i >= 0; i--)
                    {
                        if (Messages[i].Date <= msg.Date)
                        {
                            Messages.Insert(i + 1, msg);
                            break;
                        }
                        else if (i == 0)
                        {
                            Messages.Insert(0, msg);
                        }
                    }
                }
            }
            else
            {
                foreach (var msg in msgs.OrderBy(m => m.Date))
                {
                    Messages.Add(msg);
                }
            }
        }

        private void InitClient()
        {
            try
            {
                _callback = new ChatServiceCallback();
                _callback.MessagePost += callback_MessagePost;
                _callback.UsersListChange += callback_UsersListChange;

                _client = new ChatServiceClient(new InstanceContext(_callback));
                
                _client.InnerDuplexChannel.Closing += InnerChannel_Closed;
                _client.InnerDuplexChannel.Closed += InnerChannel_Closed;
                _client.InnerDuplexChannel.Faulted += InnerChannel_Closed;

                IsConnected = true;
            }
            catch
            {
                CloseClient();
                throw;
            }
        }

        private void InnerChannel_Closed(object sender, System.EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => CloseClient());
        }

        private void CloseClient()
        {
            try
            {
                if (_callback != null)
                {
                    _callback.MessagePost -= callback_MessagePost;
                    _callback.UsersListChange -= callback_UsersListChange;

                    _callback = null;
                }

                if (_client != null)
                {
                    _client.InnerDuplexChannel.Closing -= InnerChannel_Closed;
                    _client.InnerDuplexChannel.Closed -= InnerChannel_Closed;
                    _client.InnerDuplexChannel.Faulted -= InnerChannel_Closed;
                    
                    switch (_client.State)
                    {
                        case CommunicationState.Opened:
                        case CommunicationState.Opening:
                        case CommunicationState.Created:
                            _client.Abort();
                            break;
                    }

                    _client = null;
                }
            }
            catch
            {
                _callback = null;
                _client = null;
            }
            finally
            {
                _username = null;
                IsAdmin = false;
                IsConnected = false;
            }
        }
    }
}
