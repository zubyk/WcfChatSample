using System.Linq;
using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _ShowLoginWindowCommand = new ShowLoginWindowCommand();

        public static ICommand ShowLoginWindow
        {
            get
            {
                return _ShowLoginWindowCommand;
            }
        }

        private class ShowLoginWindowCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                return !App.Current.Windows.OfType<LoginWindow>().Any() && !Data.DataSource.Current.IsConnected && base.CanExecute(parameter);
            }

            public override void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    var w = new LoginWindow() { Owner = App.Current.MainWindow, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
                    w.ShowDialog();
                }
            }
        }
    }
}
