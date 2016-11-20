using System;
using System.Windows;
using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _ConnectCommand = new ConnectCommand();

        public static ICommand Connect
        {
            get
            {
                return _ConnectCommand;
            }
        }

        private class ConnectCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                var usr = parameter as Data.UserLogin;
                return usr != null && usr.IsValid && base.CanExecute(parameter);
            }

            public override void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    var usr = parameter as Data.UserLogin;
                    Data.DataSource.Current.Connect(usr.Username, usr.Password);
                }
            }
        }
    }
}
