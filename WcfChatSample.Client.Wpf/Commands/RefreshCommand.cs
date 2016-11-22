using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _RefreshCommand = new RefreshCommand();

        public static ICommand Refresh
        {
            get
            {
                return _RefreshCommand;
            }
        }

        private class RefreshCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                return base.CanExecute(parameter) && Data.DataSource.Current.IsConnected;
            }

            public override void Execute(object parameter)
            {
                Data.DataSource.Current.Refresh();
            }
        }
    }
}
