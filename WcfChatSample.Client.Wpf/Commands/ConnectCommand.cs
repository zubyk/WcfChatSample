using System.Windows.Input;
using WcfChatSample.Client.Wpf.Data;

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
                return usr != null && usr.IsValid && !Data.DataSource.Current.IsConnected && base.CanExecute(parameter);
            }

            public override void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    Data.DataSource.Current.Connect(parameter as Data.UserLogin);
                }
            }
        }
    }
}
