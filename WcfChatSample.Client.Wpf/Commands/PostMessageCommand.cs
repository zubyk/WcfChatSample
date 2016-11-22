using System;
using System.Windows.Input;

namespace WcfChatSample.Client.Wpf.Commands
{
    partial class CommandManager
    {
        private static ICommand _PostMessageCommand = new PostMessageCommand();

        public static ICommand PostMessage
        {
            get
            {
                return _PostMessageCommand;
            }
        }

        public static event EventHandler<bool> MessagePosted = delegate { };

        private class PostMessageCommand : ChatCommandBase
        {
            public override bool CanExecute(object parameter)
            {
                return !String.IsNullOrWhiteSpace(parameter as String) && base.CanExecute(parameter) && Data.DataSource.Current.IsConnected;
            }

            public override async void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    var result = await Data.DataSource.Current.Post(parameter as String);

                    CommandManager.MessagePosted(this, result);
                }
            }
        }
    }
}
