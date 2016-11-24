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
            set
            {
                OnPropertyChanged();
            }
        }
        
        public bool IsAdmin 
        { 
            get {
                return _isAdmin;
            }
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged();
                }
            } 
        }

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
                    _username = login.Username;

                    var creds = await login.SecurePassword.ProcessPasswordAsync(async (pass) => 
                    {
                        return await _client.LoginAsync(login.Username, pass);
                    });
                                        
                    IsAdmin = creds.Role == UserRole.Admin;
                }
                catch (FaultException<UserLoginFault>)
                {
                    CloseClient();
                    new Exception("Invalid login or password").ShowMessage();
                }
                catch (FaultException<ServerInternalFault>)
                {
                    CloseClient();
                    new Exception("Internal server error").ShowMessage();
                }
                catch (Exception e)
                {
                    CloseClient();
                    e.ShowMessage();
                }
                finally
                {
                    IsAsyncProcessing = false;
                }

                Refresh(false);
            }
        }

        public void Disconnect() 
        {
            if (IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    CloseClient();
                }
                catch (Exception e)
                {
                    e.ShowMessage();
                }
                finally
                {
                    Users.Clear();
                    IsAsyncProcessing = false;
                }
            }
        }

        public async void DisconnectUser(UserLogin user)
        {
            if (IsConnected && !IsAsyncProcessing && user != null && !user.IsSelf)
            {
                IsAsyncProcessing = true;
                try
                {
                    await _client.DisconnectUserAsync(user.Username);
                }
                catch (FaultException<ServerInternalFault>)
                {
                    new Exception("Internal server error").ShowMessage("User disconnect");
                }
                catch (FaultException<UserLoginRequiredFault>)
                {
                    CloseClient();
                    new Exception("User must log in first").ShowMessage("User disconnect");
                }
                catch (FaultException)
                {
                    CloseClient();
                    new Exception("Session timeout").ShowMessage("User disconnect");
                }
                catch (Exception e)
                {
                    e.ShowMessage("User disconnect");
                }
                finally
                {
                    IsAsyncProcessing = false;
                }
            }
        }

        public async void Refresh(bool clear = true)
        {
            if (IsConnected && !IsAsyncProcessing)
            {
                IsAsyncProcessing = true;
                try
                {
                    var msgs = await _client.RefreshAsync();
                    
                    lock (_messages_lock)
                    {
                        if (clear)
                        {
                            Messages = new ObservableCollection<ChatMessage>(msgs.OrderBy(m => m.Date));
                        }
                        else
                        {
                            AddMessages(msgs);
                        }
                    }

                    OnPropertyChanged("Messages");
                }
                catch (FaultException<ServerInternalFault>)
                {
                    new Exception("Internal server error").ShowMessage();
                }
                catch (FaultException<UserLoginRequiredFault>)
                {
                    CloseClient();
                    new Exception("User must log in first").ShowMessage();
                }
                catch (FaultException)
                {
                    CloseClient();
                    new Exception("Session timeout").ShowMessage();
                }
                catch (Exception e)
                {
                    e.ShowMessage();
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
                    await _client.PostAsync(message);
                    return true;
                }
                catch (FaultException<ServerInternalFault>)
                {
                    new Exception("Internal server error").ShowMessage();
                }
                catch (FaultException<UserLoginRequiredFault>)
                {
                    CloseClient();
                    new Exception("User must log in first").ShowMessage();
                }
                catch (FaultException)
                {
                    CloseClient();
                    new Exception("Session timeout").ShowMessage();
                }
                catch (Exception e)
                {
                    e.ShowMessage();
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
