using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using WcfChatSample.Client.Wpf.ServiceReference;

namespace WcfChatSample.Client.Wpf.Data
{
    public partial class DataSource : INotifyPropertyChanged
    {
        private ChatServiceClient _client;
        private ChatServiceCallback _callback;  

        private bool _isAsyncProcessing = false;
        private string _key;
        private string _username;   

        private object _messages_lock = new object();
        private object _users_lock = new object();

        public ObservableCollection<ChatMessage> Messages { get; private set; }
        public ObservableCollection<UserEntity> Users { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private DataSource()
        {
            Messages = new ObservableCollection<ChatMessage>();
            Users = new ObservableCollection<UserEntity>();
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void callback_UsersListChange(object sender, string[] e)
        {
            lock (_users_lock)
            {
                Users = new ObservableCollection<UserEntity>(e.OrderBy(u => u).Select(u => new UserEntity(u, u == _username)));
            }
            OnPropertyChanged("Users");
        }

        private void callback_MessagePost(object sender, ChatMessage[] e)
        {
            if (e.Length > 0)
            {
                lock (_messages_lock)
                {
                    if (Messages.Any())
                    {
                        foreach (var msg in e.OrderBy(m => m.Date))
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
                        foreach (var msg in e.OrderBy(m => m.Date))
                        {
                            Messages.Add(msg);
                        }
                    }
                }

                OnPropertyChanged("Messages");
            }
        }

        private void InitClient()
        {           
            _callback = new ChatServiceCallback();
            _callback.MessagePost += callback_MessagePost;
            _callback.UsersListChange += callback_UsersListChange;

            _client = new ChatServiceClient(new InstanceContext(_callback));
        }

        private void CloseClient()
        {
            if (_callback != null)
            {
                _callback.MessagePost -= callback_MessagePost;
                _callback.UsersListChange -= callback_UsersListChange;
            }

            if (_client != null && (
                _client.State == CommunicationState.Created || 
                _client.State == CommunicationState.Opened || 
                _client.State == CommunicationState.Opening))
            {
                _client.Close();
            }

            _client = null;
            _callback = null;
        }
    }
}
