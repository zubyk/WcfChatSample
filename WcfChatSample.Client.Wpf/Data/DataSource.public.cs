using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using WcfChatSample.Client.Wpf.Extensions;
using WcfChatSample.Client.Wpf.ServiceReference;

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

        public async void Connect(UserLogin login)
        {
            if (!IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    InitClient();

                    var creds = await login.SecurePassword.ProcessPasswordAsync(async (pass) => 
                    {
                        return await _client.LoginAsync(login.Username, pass);
                    });
                    
                    _key = creds.Key;
                    _username = login.Username;
                    IsAdmin = creds.IsAdmin;
                    OnPropertyChanged("IsAdmin");
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

                Refresh();
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
                    
                    lock (_messages_lock)
                    {
                        Messages = new ObservableCollection<ChatMessage>(msgs.OrderBy(m => m.Date));
                    }

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

        public async Task<bool> Post(string message)
        {
            if (IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    await _client.PostAsync(_key, message);
                    return true;
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

            return false;
        }
    }
}
