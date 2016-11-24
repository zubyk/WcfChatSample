using System;
using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _DisconnectCommand = new DisconnectCommand();

        public static ICommand Disconnect
        {
            get
            {
                return _DisconnectCommand;
            }
        }

        private class DisconnectCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter) && Data.DataSource.Current.IsConnected;
            }
            
            public override void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    Data.DataSource.Current.Disconnect();
                }
            }
        }
    }
}
