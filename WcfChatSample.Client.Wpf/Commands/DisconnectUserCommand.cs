using System;
using System.Windows.Input;
using WcfChatSample.Client.Wpf.Data;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _DisconnectUserCommand = new DisconnectUserCommand();

        public static ICommand DisconnectUser
        {
            get
            {
                return _DisconnectUserCommand;
            }
        }

        private class DisconnectUserCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                return parameter is UserLogin && !(parameter as UserLogin).IsSelf && base.CanExecute(parameter) && Data.DataSource.Current.IsConnected;
            }
            
            public override void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    Data.DataSource.Current.DisconnectUser(parameter as UserLogin);
                }
            }
        }
    }
}
