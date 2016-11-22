using System;
using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    internal partial class CommandManager
    {
        public CommandManager()
        {
            
        }

        public static void FireCanExecuteChanged(ICommand command)
        {
            if (command is ChatCommandBase)
            {
                (command as ChatCommandBase).FireCanExecuteChanged();
            }
        }
        
        private class ChatCommandBase : ICommand
        {           
            public virtual bool CanExecute(object parameter)
            {
                return !Data.DataSource.Current.IsAsyncProcessing;
            }

            public virtual void Execute(object parameter)
            {

            }
            
            public event EventHandler CanExecuteChanged = delegate { };

            public void FireCanExecuteChanged()
            {
                CanExecuteChanged(this, new EventArgs());
            }

            public ChatCommandBase()
            {
                Data.DataSource.Current.PropertyChanged += Current_PropertyChanged;
            }

            ~ChatCommandBase()
            {
                Data.DataSource.Current.PropertyChanged -= Current_PropertyChanged;
            }

            private void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "IsConnected":
                    case "IsAsyncProcessing":
                        CanExecuteChanged(this, new EventArgs());
                        break;
                }
            }
        }
    }
}
