using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Windows;
using System.Reflection;
using WcfChatSample.Client.Wpf.ServiceReference;
using WcfChatSample.Client.Wpf.Extensions;
using System.ServiceModel;

namespace WcfChatSample.Client.Wpf.Data
{
    partial class DataSource
    {   
        public string ProductName
        {
            get
            {
                foreach(var a in Application.Current.MainWindow.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true))
                {
                    return (a as AssemblyProductAttribute).Product;
                }

                return String.Empty;
            }
        }

        public string ProductVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public bool IsConnected
        {
            get
            {
                return _client != null;
            }
        }
        
        public bool IsAdmin { get; set; }

        public bool IsAsyncProcessing
        {
            get
            {
                return _isAsyncProcessing;
            }
            set
            {
                if (value != _isAsyncProcessing)
                {
                    _isAsyncProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        public async void Connect(string username, string password)
        {
            if (!IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    InitClient();

                    var creds =  await _client.LoginAsync(username, password);

                    _key = creds.Key;
                    IsAdmin = creds.IsAdmin;
                }
                catch (FaultException<UserLoginFault>)
                {
                    CloseClient();
                    new Exception("Invalid login or password").ShowMessage("Log in");
                }
                catch (FaultException<ServerInternalFault>)
                {
                    CloseClient();
                    new Exception("Internal server error").ShowMessage("Server");
                }
                catch (Exception e)
                {
                    CloseClient();
                    e.ShowMessage("Server connect");
                }
                finally
                {
                    IsAsyncProcessing = false;
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        public async void Disconnect(string username = null)
        {
            if (IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    await _client.DisconnectUserAsync(_key, username);
                }
                catch (Exception)
                {
                }
                finally
                {
                    CloseClient();
                    IsAsyncProcessing = false;
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        public async void Refresh()
        {
            if (IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    var msgs = await _client.RefreshAsync(_key);
                    Messages = new ObservableCollection<ChatMessage>(msgs);
                    OnPropertyChanged("Messages");
                }
                catch (FaultException<ServerInternalFault>)
                {
                    new Exception("Internal server error").ShowMessage("Refresh");
                }
                catch (FaultException)
                {
                    CloseClient();
                    new Exception("Session timeout").ShowMessage("Refresh");
                    OnPropertyChanged("IsConnected");
                }
                catch (Exception e)
                {
                    e.ShowMessage("Refresh");
                }
                finally
                {
                    IsAsyncProcessing = false;
                }
            }
        }

        public async void Post(string message)
        {
            if (IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    await _client.PostAsync(_key, message);
                }
                catch (FaultException<ServerInternalFault>)
                {
                    new Exception("Internal server error").ShowMessage("Post");
                }
                catch (FaultException)
                {
                    CloseClient();
                    new Exception("Session timeout").ShowMessage("Post");
                    OnPropertyChanged("IsConnected");
                }
                catch (Exception e)
                {
                    e.ShowMessage("Post");
                }
                finally
                {
                    IsAsyncProcessing = false;
                }
            }
        }
    }
}
