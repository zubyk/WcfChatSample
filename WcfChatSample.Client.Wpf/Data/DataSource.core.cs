using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Collections;
using System.IO;
using WcfChatSample.Client.Wpf.ServiceReference;

namespace WcfChatSample.Client.Wpf.Data
{
    public partial class DataSource : INotifyPropertyChanged
    {
        private ChatServiceClient _client;
        private ChatServiceCallback _callback;  

        private bool _isAsyncProcessing = false;
        private string _key;

        public ObservableCollection<ChatMessage> Messages { get; private set; }
        public ObservableCollection<string> Users { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private DataSource()
        {
            Messages = new ObservableCollection<ChatMessage>();
            Users = new ObservableCollection<string>();
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void callback_UsersListChange(object sender, string[] e)
        {
            Users = new ObservableCollection<string>(e);
            OnPropertyChanged("Users");
        }

        private void callback_MessagePost(object sender, ChatMessage[] e)
        {
            foreach(var m in e)
            {
                Messages.Add(m);
            }
        }

        private void InitClient()
        {
            _callback = new ChatServiceCallback();
            _callback.MessagePost += callback_MessagePost;
            _callback.UsersListChange += callback_UsersListChange;

            _client = new ChatServiceClient(new System.ServiceModel.InstanceContext(_callback));
        }

        private void CloseClient()
        {
            if (_client != null)
            {
                _client.Close();
            }

            if (_callback != null)
            {
                _callback.MessagePost -= callback_MessagePost;
                _callback.UsersListChange -= callback_UsersListChange;
            }

            _client = null;
            _callback = null;
        }
    }
}
